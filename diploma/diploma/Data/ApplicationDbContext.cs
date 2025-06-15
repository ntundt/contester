using diploma.Features.Attempts;
using diploma.Features.Authentication;
using diploma.Features.Contests;
using diploma.Features.Problems;
using diploma.Features.SchemaDescriptions;
using diploma.Features.Users;
using Microsoft.EntityFrameworkCore;
using diploma.Features.AttachedFiles;
using diploma.Features.ContestApplications;
using diploma.Features.Scoreboard;
using Microsoft.AspNetCore.Identity;
using diploma.Data.Common;
using System.Text.Json;
using diploma.Features.ApplicationSettings;
using diploma.Features.Grade;

namespace diploma.Data;

public interface IApplicationDbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Contest> Contests { get; set; }
    public DbSet<Problem> Problems { get; set; }
    public DbSet<Attempt> Attempts { get; set; }
    public DbSet<GradeAdjustment> GradeAdjustments { get; set; }
    public DbSet<ContestApplication> ContestApplications { get; set; }
    public DbSet<ScoreboardApproval> ScoreboardApprovals { get; set; }
    
    public DbSet<SchemaDescription> SchemaDescriptions { get; set; }
    public DbSet<SchemaDescriptionFile> SchemaDescriptionFiles { get; set; }

    public DbSet<AttachedFile> AttachedFiles { get; set; }
    
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    
    public DbSet<Audit> AuditEntries { get; set; }
    
    public DbSet<ConnectionString> ConnectionStrings { get; set; }
}

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    Features.Authentication.Services.IAuthorizationService authorizationService,
    ILogger<ApplicationDbContext> logger)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Contest> Contests { get; set; } = null!;
    public DbSet<Problem> Problems { get; set; } = null!;
    public DbSet<Attempt> Attempts { get; set; } = null!;
    public DbSet<GradeAdjustment> GradeAdjustments { get; set; } = null!;
    public DbSet<ContestApplication> ContestApplications { get; set; } = null!;
    public DbSet<ScoreboardApproval> ScoreboardApprovals { get; set; } = null!;

    public DbSet<SchemaDescription> SchemaDescriptions { get; set; } = null!;
    public DbSet<SchemaDescriptionFile> SchemaDescriptionFiles { get; set; } = null!;

    public DbSet<AttachedFile> AttachedFiles { get; set; } = null!;
    
    public DbSet<UserRole> UserRoles { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;

    public DbSet<Audit> AuditEntries { get; set; } = null!;

    public DbSet<ConnectionString> ConnectionStrings { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new AuditableInterceptor());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasMany<Contest>(u => u.ContestsUserParticipatesIn)
            .WithMany(c => c.Participants)
            .UsingEntity("ContestParticipants");

        modelBuilder.Entity<Contest>()
            .HasMany<User>(c => c.CommissionMembers)
            .WithMany()
            .UsingEntity("ContestCommissionMembers");
        
        modelBuilder.Entity<Contest>()
            .HasOne<User>(c => c.Author)
            .WithMany(u => u.AuthoredContests);

        modelBuilder.Entity<Permission>().HasData(
            new Permission { Id = 1, Name = Constants.Permission.ManageContests.ToString() },
            new Permission { Id = 2, Name = Constants.Permission.ManageProblems.ToString() },
            new Permission { Id = 3, Name = Constants.Permission.ManageAttempts.ToString() },
            new Permission { Id = 4, Name = Constants.Permission.ManageContestParticipants.ToString() },
            new Permission { Id = 5, Name = Constants.Permission.ManageSchemaDescriptions.ToString() }
        );
        modelBuilder.Entity<UserRole>().HasData(
            new UserRole { Id = 1, Name = "Admin" },
            new UserRole { Id = 2, Name = "User" }
        );
        
        modelBuilder.Entity<UserRole>()
            .HasMany(ur => ur.Permissions)
            .WithMany(c => c.UserRoles)
            .UsingEntity(j => j.HasData(
                new { UserRolesId = 1, PermissionsId = 1 },
                new { UserRolesId = 1, PermissionsId = 2 },
                new { UserRolesId = 1, PermissionsId = 3 },
                new { UserRolesId = 1, PermissionsId = 4 },
                new { UserRolesId = 1, PermissionsId = 5 }
            ));
        
        var user = new User {
            Id = new Guid("9a47a812-35f4-4c70-a44e-bd3ff2d00cda"),
            Email = "admin@contest.er",
            FirstName = "Admin",
            LastName = "",
            AdditionalInfo = "",
            PasswordHash = "",
            UserRoleId = 1,
            EmailConfirmationCode = "",
            IsEmailConfirmed = true,
        };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, "admin");
        modelBuilder.Entity<User>().HasData(user);

        modelBuilder.Entity<ConnectionString>().HasData(
            new { Id = 1, Text = "Data Source=oracle_db:1521/xe;User Id=SQL_CONTEST_USER;Password=Password123;", Dbms = "Oracle" },
            new { Id = 2, Text = "Server=postgres_db;Port=5432;Database=sql_contest;User Id=sql_contest_user;Password=Password123;", Dbms = "Postgres" },
            new { Id = 3, Text = "Server=sql_server_db;Database=SQL_CONTEST;User Id=SQL_CONTEST_USER;Password=Password123;", Dbms = "SqlServer" }
        );
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        TryOnBeforeSaveChanges();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        TryOnBeforeSaveChanges();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        TryOnBeforeSaveChanges();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    private void TryOnBeforeSaveChanges()
    {
        try 
        {
            OnBeforeSaveChanges();
        }
        catch (Exception e)
        {
            logger.LogWarning("Could not add audit entry\n{}", e.ToString());
        }
    }

    private void OnBeforeSaveChanges()
    {
        // datetime on auditableEntity is updated by AuditableInterceptor
        // Here we add a respective audit entry
        var auditEntries = ChangeTracker.Entries<AuditableEntity>()
            .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified || x.State == EntityState.Deleted);
        foreach (var entry in auditEntries.ToList())
        {
            var oldValues = new[] {EntityState.Deleted, EntityState.Modified}.Contains(entry.State)
                ? JsonSerializer.Serialize(entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue)) : "";
            var newValues = new[] {EntityState.Added, EntityState.Modified}.Contains(entry.State)
                ? JsonSerializer.Serialize(entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue)) : "";
            var auditEntry = new Audit
            {
                UserId = authorizationService.TryGetUserId() ?? Guid.Empty,
                Type = entry.State.ToString(),
                TableName = entry.Metadata?.GetTableName() ?? entry.Entity.GetType().Name,
                Date = DateTime.UtcNow,
                OldValues = oldValues,
                NewValues = newValues,
            };
            AuditEntries.Add(auditEntry);
        }
    }
}
