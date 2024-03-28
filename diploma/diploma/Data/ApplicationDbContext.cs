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

namespace diploma.Data;

public interface IApplicationDbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Contest> Contests { get; set; }
    public DbSet<Problem> Problems { get; set; }
    public DbSet<Attempt> Attempts { get; set; }
    public DbSet<GradeAdjustment> GradeAdjustments { get; set; }
    
    public DbSet<SchemaDescription> SchemaDescriptions { get; set; }
    public DbSet<SchemaDescriptionFile> SchemaDescriptionFiles { get; set; }
    
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Claim> Claims { get; set; }
}

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Contest> Contests { get; set; } = null!;
    public DbSet<Problem> Problems { get; set; } = null!;
    public DbSet<Attempt> Attempts { get; set; } = null!;
    public DbSet<GradeAdjustment> GradeAdjustments { get; set; } = null!;
    public DbSet<ContestApplication> ContestApplications { get; set; } = null!;
    
    public DbSet<SchemaDescription> SchemaDescriptions { get; set; } = null!;
    public DbSet<SchemaDescriptionFile> SchemaDescriptionFiles { get; set; } = null!;

    public DbSet<AttachedFile> AttachedFiles { get; set; } = null!;
    
    public DbSet<UserRole> UserRoles { get; set; } = null!;
    public DbSet<Claim> Claims { get; set; } = null!;

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

        modelBuilder.Entity<Claim>().HasData(
            new Claim { Id = 1, Name = "ManageContests" },
            new Claim { Id = 2, Name = "ManageProblems" },
            new Claim { Id = 3, Name = "ManageAttempts" },
            new Claim { Id = 4, Name = "ManageContestParticipants" },
            new Claim { Id = 5, Name = "ManageSchemaDescriptions" }
        );
        modelBuilder.Entity<UserRole>().HasData(
            new UserRole { Id = 1, Name = "Admin" },
            new UserRole { Id = 2, Name = "User" }
        );
        
        modelBuilder.Entity<UserRole>()
            .HasMany(ur => ur.Claims)
            .WithMany(c => c.UserRoles)
            .UsingEntity(j => j.HasData(
                new { UserRolesId = 1, ClaimsId = 1 },
                new { UserRolesId = 1, ClaimsId = 2 },
                new { UserRolesId = 1, ClaimsId = 3 },
                new { UserRolesId = 1, ClaimsId = 4 },
                new { UserRolesId = 1, ClaimsId = 5 }
            ));
    }
}