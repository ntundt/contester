using contester.Features.ApplicationSettings;
using contester.Features.AttachedFiles;
using contester.Features.Attempts;
using contester.Features.Audit;
using contester.Features.Authentication;
using contester.Features.ContestApplications;
using contester.Features.Contests;
using contester.Features.Grade;
using contester.Features.Problems;
using contester.Features.SchemaDescriptions;
using contester.Features.Scoreboard;
using contester.Features.Users;
using contester.Infrastructure.Seeders;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace contester.Infrastructure.Persistence;

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
    IConfigurationReaderService configuration,
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

    public DbSet<ScoreboardEntry> ScoreboardEntries { get; init; }= null!;

    public async Task<int> RefreshScoreboardEntriesAsync()
    {
        var viewName = Model.FindEntityType(typeof(ScoreboardEntry))!.GetViewName();
        var sql = $"REFRESH MATERIALIZED VIEW \"{viewName}\";";
        return await Database.ExecuteSqlRawAsync(sql);
    }

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
        
        modelBuilder.Entity<ScoreboardEntry>()
            .ToView("Scoreboard")
            .HasNoKey()
            .Property(se => se.Problems)
            .HasConversion(
                se => JsonConvert.SerializeObject(se, Formatting.Indented),
                se => JsonConvert.DeserializeObject<List<ScoreboardProblemEntry>>(se)!
            );

        DataSeeder.SeedData(modelBuilder, configuration);
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
