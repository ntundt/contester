using diploma.Features.Attempts;
using diploma.Features.Authentication;
using diploma.Features.Contests;
using diploma.Features.Problems;
using diploma.Features.SchemaDescriptions;
using diploma.Features.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Duende.IdentityServer.EntityFramework.Options;
using diploma.Features.AttachedFiles;
using diploma.Features.ContestApplications;
using diploma.Features.GradeAdjustments;
using diploma.Features.Scoreboard;
using Microsoft.AspNetCore.Identity;

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
}

public class ApplicationDbContext : DbContext, IApplicationDbContext
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

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, 
            IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options) { }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new AuditableInterceptor());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasMany<Contest>(u => u.ContestsUserParticipatesIn)
            .WithMany(c => c.Participants);

        modelBuilder.Entity<Contest>()
            .HasMany<User>(c => c.CommissionMembers)
            .WithMany();
        
        modelBuilder.Entity<Contest>()
            .HasOne<User>(c => c.Author)
            .WithMany(u => u.AuthoredContests);

        modelBuilder.Entity<Permission>().HasData(
            new Permission { Id = 1, Name = "ManageContests" },
            new Permission { Id = 2, Name = "ManageProblems" },
            new Permission { Id = 3, Name = "ManageAttempts" },
            new Permission { Id = 4, Name = "ManageContestParticipants" },
            new Permission { Id = 5, Name = "ManageSchemaDescriptions" }
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
            Id = Guid.NewGuid(),
            Email = "admin@contest.er",
            FirstName = "Admin",
            LastName = "",
            AdditionalInfo = "",
            PasswordHash = "",
            UserRoleId = 1,
        };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, "admin");
        modelBuilder.Entity<User>().HasData(user);
    }
}