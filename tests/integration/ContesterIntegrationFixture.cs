using contester.Features.ApplicationSettings;
using contester.Features.ContestApplications;
using contester.Features.Contests;
using contester.Features.Problems;
using contester.Features.SchemaDescriptions;
using contester.Features.UserGroups;
using contester.Features.Users;
using contester.Infrastructure.Databases;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace tests.integration;

public sealed class ContesterIntegrationFixture : IAsyncDisposable
{
    public PostgreSqlContainer DataContainer { get; }
    public PostgreSqlContainer ExecutionContainer { get; }
    public ContesterWebApplicationFactory Factory { get; private set; } = null!;
    public string ApplicationDirectoryPath { get; }
    public string AssetsPath { get; }

    public Guid AdminUserId { get; private set; }
    public Guid ContestantUserId { get; private set; }
    public Guid OtherUserId { get; private set; }
    public Guid ContestId { get; private set; }
    public Guid ProblemId { get; private set; }
    public Guid SchemaDescriptionId { get; private set; }
    public Guid SchemaDescriptionFileId { get; private set; }
    public Guid ExtraGroupId { get; private set; }
    public Guid PendingApplicationId { get; private set; }

    public ContesterIntegrationFixture()
    {
        ApplicationDirectoryPath = Path.Combine(Path.GetTempPath(), "contester-integration", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(ApplicationDirectoryPath);

        AssetsPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "Assets"));

        DataContainer = new PostgreSqlBuilder("postgres:18")
            .WithDatabase("data")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithCleanUp(true)
            .Build();

        ExecutionContainer = new PostgreSqlBuilder("postgres:18")
            .WithDatabase("test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithCleanUp(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await Task.WhenAll(DataContainer.StartAsync(), ExecutionContainer.StartAsync());

        Environment.SetEnvironmentVariable(
            "Contester_ConnectionStrings__DefaultConnection",
            DataContainer.GetConnectionString());
        Environment.SetEnvironmentVariable(
            "Contester_ConnectionStrings__PostgresAdminConnection",
            DataContainer.GetConnectionString());
        Environment.SetEnvironmentVariable(
            "Contester_ApplicationDirectoryPath",
            ApplicationDirectoryPath);

        Factory = new ContesterWebApplicationFactory(
            DataContainer.GetConnectionString(),
            ApplicationDirectoryPath);

        _ = Factory.Services;

        await SeedAsync();
    }

    private async Task SeedAsync()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        AdminUserId = Guid.NewGuid();
        ContestantUserId = Guid.NewGuid();
        OtherUserId = Guid.NewGuid();
        ContestId = Guid.NewGuid();
        ProblemId = Guid.NewGuid();
        SchemaDescriptionId = Guid.NewGuid();
        SchemaDescriptionFileId = Guid.NewGuid();
        ExtraGroupId = Guid.NewGuid();
        PendingApplicationId = Guid.NewGuid();
        var participantsGroupId = Guid.NewGuid();

        var admin = new User
        {
            Id = AdminUserId,
            Email = "admin@integration.test",
            FirstName = "Admin",
            LastName = "User",
            AdditionalInfo = "",
            PasswordHash = "hash",
            UserRoleId = 1,
            EmailConfirmationCode = "",
            IsEmailConfirmed = true,
        };
        var contestant = new User
        {
            Id = ContestantUserId,
            Email = "contestant@integration.test",
            FirstName = "Contestant",
            LastName = "User",
            AdditionalInfo = "",
            PasswordHash = "hash",
            UserRoleId = 2,
            EmailConfirmationCode = "",
            IsEmailConfirmed = true,
        };
        var other = new User
        {
            Id = OtherUserId,
            Email = "other@integration.test",
            FirstName = "Other",
            LastName = "User",
            AdditionalInfo = "",
            PasswordHash = "hash",
            UserRoleId = 2,
            EmailConfirmationCode = "",
            IsEmailConfirmed = true,
        };

        context.Users.AddRange(admin, contestant, other);

        context.UserGroups.AddRange(
            new UserGroup
            {
                Id = participantsGroupId,
                Name = $"Contest {ContestId} participants",
                IsSystemGroup = true,
                MemberUsers = [contestant],
            },
            new UserGroup
            {
                Id = ExtraGroupId,
                Name = "Extra participant group",
                IsSystemGroup = false,
                MemberUsers = [other],
            });

        context.Contests.Add(new Contest
        {
            Id = ContestId,
            Name = "Integration Contest",
            DescriptionPath = Path.Combine(AssetsPath, "ContestDescription.md"),
            AuthorId = AdminUserId,
            StartDate = DateTime.UtcNow.AddDays(-1),
            FinishDate = DateTime.UtcNow.AddDays(1),
            IsPublic = true,
            ParticipantsGroupId = participantsGroupId,
            CommissionMembers = [admin],
        });

        context.SchemaDescriptions.Add(new SchemaDescription
        {
            Id = SchemaDescriptionId,
            Name = "Employees schema",
            ContestId = ContestId,
        });

        context.SchemaDescriptionFiles.Add(new SchemaDescriptionFile
        {
            Id = SchemaDescriptionFileId,
            SchemaDescriptionId = SchemaDescriptionId,
            Dbms = "Postgres",
            FilePath = Path.Combine(AssetsPath, "Schema.sql"),
        });

        context.Problems.Add(new Problem
        {
            Id = ProblemId,
            Name = "High salary employees",
            StatementPath = Path.Combine(AssetsPath, "ProblemStatement.md"),
            OrderMatters = true,
            FloatMaxDelta = 0.01m,
            CaseSensitive = true,
            TimeLimit = TimeSpan.FromSeconds(5),
            MaxGrade = 10,
            Ordinal = 1,
            ContestId = ContestId,
            SchemaDescriptionId = SchemaDescriptionId,
            SolutionPath = Path.Combine(AssetsPath, "Solution.sql"),
            SolutionDbms = "Postgres",
        });

        context.ContestApplications.Add(new ContestApplication
        {
            Id = PendingApplicationId,
            ContestId = ContestId,
            UserId = OtherUserId,
            IsApproved = false,
        });

        var connectionStrings = await context.ConnectionStrings.ToListAsync();
        foreach (var connectionString in connectionStrings)
            context.ConnectionStrings.Remove(connectionString);

        context.ConnectionStrings.Add(new ConnectionString
        {
            Id = 1,
            Dbms = "Postgres",
            Text = ExecutionContainer.GetConnectionString(),
        });

        await context.SaveChangesAsync();

        ConnectionStringsCache.Instance.SetCachedValues(
            await context.ConnectionStrings.AsNoTracking().ToListAsync());
    }

    public async Task<T> SendAsync<T>(IRequest<T> request, Guid asUserId)
    {
        Factory.CurrentUserId = asUserId;
        await using var scope = Factory.Services.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        return await mediator.Send(request);
    }

    public string ReadAsset(string fileName) =>
        File.ReadAllText(Path.Combine(AssetsPath, fileName));

    public async ValueTask DisposeAsync()
    {
        if (Factory is not null)
            await Factory.DisposeAsync();

        await DataContainer.DisposeAsync();
        await ExecutionContainer.DisposeAsync();

        Environment.SetEnvironmentVariable("Contester_ConnectionStrings__DefaultConnection", null);
        Environment.SetEnvironmentVariable("Contester_ConnectionStrings__PostgresAdminConnection", null);
        Environment.SetEnvironmentVariable("Contester_ApplicationDirectoryPath", null);

        try
        {
            if (Directory.Exists(ApplicationDirectoryPath))
                Directory.Delete(ApplicationDirectoryPath, recursive: true);
        }
        catch
        {
            // Best-effort cleanup of temp storage.
        }
    }
}
