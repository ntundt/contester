using diploma.Data;
using diploma.Features.Attempts;
using diploma.Features.Contests;
using diploma.Features.Problems;
using diploma.Features.SchemaDescriptions;
using diploma.Features.Users;
using diploma.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace tests.Services;

[TestFixture]
public class SolutionRunnerTests
{
    private ApplicationDbContext? _context;
    private IConfiguration? _configuration;
    private IConfigurationReaderService? _configurationReaderService;
    private IFileService? _fileService;
    private IDirectoryService? _directoryService;
    
    [SetUp]
    public void Setup()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlite(connection);
        _context = new ApplicationDbContext(optionsBuilder.Options, null!, new LoggerFactory().CreateLogger<ApplicationDbContext>());
        _context.Database.Migrate();

        var schemaDescriptionId = Guid.NewGuid();
        var contestId = Guid.NewGuid();
        var problems = new List<Problem>()
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Problem 1",
                StatementPath = Path.Combine(Constants.AssetsBasePath, "Statement.md"),
                OrderMatters = true,
                FloatMaxDelta = 0.01m,
                CaseSensitive = true,
                TimeLimit = TimeSpan.FromSeconds(1),
                SchemaDescriptionId = schemaDescriptionId,
                SolutionPath = Path.Combine(Constants.AssetsBasePath, "Solution.sql"),
                SolutionDbms = "SqlServer",
                ContestId = contestId,
            }
        };
        
        var schemaDescriptions = new List<SchemaDescription>()
        {
            new()
            {
                Id = schemaDescriptionId,
                Name = "Schema description 1",
            }
        };
        
        var schemaDescriptionFiles = new List<SchemaDescriptionFile>()
        {
            new()
            {
                Id = Guid.NewGuid(),
                FilePath = Path.Combine(Constants.AssetsBasePath, "Schema.sql"),
                Dbms = "SqlServer",
                SchemaDescriptionId = schemaDescriptions.First().Id,
            }
        };

        var users = new List<User>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Email = "nikita.tihonovich@gmail.com",
                FirstName = "Nikita",
                LastName = "Tsikhanovich",
                Patronymic = "Piatrovich",
                AdditionalInfo = "",
                EmailConfirmationToken = Guid.NewGuid(),
                EmailConfirmationTokenExpiresAt = DateTime.UtcNow.AddDays(1),
                IsEmailConfirmed = true,
                PasswordHash = "1234567890",
                PasswordRecoveryToken = Guid.NewGuid(),
                PasswordRecoveryTokenExpiresAt = DateTime.UtcNow.AddDays(1),
                UserRoleId = 1,
                EmailConfirmationCode = "",
            }
        };

        var contests = new List<Contest>
        {
            new()
            {
                Id = contestId,
                Name = "Contest 1",
                DescriptionPath = Path.Combine(Constants.AssetsBasePath, "ContestDescription.md"),
                AuthorId = users.First().Id,
                StartDate = DateTime.Today,
                FinishDate = DateTime.Today.AddDays(1),
                IsPublic = true,
            }
        };

        _context.Problems.AddRange(problems);
        _context.SchemaDescriptions.AddRange(schemaDescriptions);
        _context.SchemaDescriptionFiles.AddRange(schemaDescriptionFiles);
        _context.Users.AddRange(users);
        _context.Contests.AddRange(contests);
        _context.SaveChanges();
        
        _configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Development.json")
            .Build();

        _configurationReaderService = new ConfigurationReaderService(_configuration);
        
        _directoryService = new DirectoryService(_configurationReaderService, new LoggerFactory().CreateLogger<DirectoryService>());
        
        _fileService = new FileService(_configuration, _directoryService);
    }
    
    private async Task AddSolutionAssetToDbAsync(Guid attemptId, string assetFilename, CancellationToken cancellationToken = default)
    {
        var attempt = new Attempt
        {
            Id = attemptId,
            ProblemId = _context!.Problems.First().Id,
            SolutionPath = Path.Combine(Constants.AssetsBasePath, assetFilename),
            Dbms = "SqlServer",
            AuthorId = _context.Users.First().Id,
            Status = AttemptStatus.Pending,
        };
        await _context!.AddAsync(attempt, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
    
    [Test]
    public async Task RunSolutionAsync_WhenCalled_ReturnsAcceptedStatus()
    {
        var runner = new SolutionCheckerService(_context!, _directoryService!, _fileService!, _configuration!);
        var attemptGuid = Guid.NewGuid();
        await AddSolutionAssetToDbAsync(attemptGuid, "ContestantSolution.sql", CancellationToken.None);
        
        var (result, _) = await runner.RunAsync(attemptGuid, CancellationToken.None);
        
        Assert.That(result, Is.EqualTo(AttemptStatus.Accepted));
    }
    
    [Test]
    public async Task RunSolutionAsync_WhenCalled_ReturnsWrongAnswerStatus()
    {
        var runner = new SolutionCheckerService(_context!, _directoryService!, _fileService!, _configuration!);
        var attemptGuid = Guid.NewGuid();
        await AddSolutionAssetToDbAsync(attemptGuid, "ContestantSolutionWrongAnswer.sql", CancellationToken.None);
        
        var (result, _) = await runner.RunAsync(attemptGuid, CancellationToken.None);
        
        Assert.That(result, Is.EqualTo(AttemptStatus.WrongAnswer));
    }
    
    [Test]
    public async Task RunSolutionAsync_WhenCalled_ReturnsWrongOutputFormatStatus()
    {
        var runner = new SolutionCheckerService(_context!, _directoryService!, _fileService!, _configuration!);
        var attemptGuid = Guid.NewGuid();
        await AddSolutionAssetToDbAsync(attemptGuid, "ContestantSolutionWrongFormat.sql", CancellationToken.None);
        
        var (result, _) = await runner.RunAsync(attemptGuid, CancellationToken.None);
        
        Assert.That(result, Is.EqualTo(AttemptStatus.WrongOutputFormat));
    }

    [Test]
    public async Task RunSolutionAsync_WhenCalled_ReturnsSyntaxErrorStatus()
    {
        var runner = new SolutionCheckerService(_context!, _directoryService!, _fileService!, _configuration!);
        var attemptGuid = Guid.NewGuid();
        await AddSolutionAssetToDbAsync(attemptGuid, "ContestantSolutionSyntaxError.sql", CancellationToken.None);
        
        var (result, _) = await runner.RunAsync(attemptGuid, CancellationToken.None);
        
        Assert.That(result, Is.EqualTo(AttemptStatus.SyntaxError));
    }

    [Test]
    public async Task RunSolutionAsync_WhenCalled_DoesNotThrowConversionFromDBNullException()
    {
        var runner = new SolutionCheckerService(_context!, _directoryService!, _fileService!, _configuration!);
        var attemptGuid = Guid.NewGuid();
        await AddSolutionAssetToDbAsync(attemptGuid, "ContestantSolutionWithNulls.sql", CancellationToken.None);
        
        var (result, _) = await runner.RunAsync(attemptGuid, CancellationToken.None);
        
        Assert.That(result, Is.EqualTo(AttemptStatus.WrongAnswer));
    }

    [TearDown]
    public void TearDown()
    {
        _context?.Dispose();
    }
}
