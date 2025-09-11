using contester.Features.ApplicationSettings;
using contester.Features.Authentication;
using contester.Features.Users;
using contester.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace contester.Data;

public static class DataSeeder
{
    public static void SeedData(ModelBuilder modelBuilder, IConfigurationReaderService configuration)
    {
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
        
        modelBuilder.Entity<ConnectionString>().HasData(
            new { Id = 1, Text = "Data Source=oracle_db:1521/xe;User Id=SQL_CONTEST_USER;Password=Password123;", Dbms = "Oracle" },
            new { Id = 2, Text = "Server=postgres_db;Port=5432;Database=sql_contest;User Id=sql_contest_user;Password=Password123;", Dbms = "Postgres" },
            new { Id = 3, Text = "Server=sql_server_db;Database=SQL_CONTEST;User Id=SQL_CONTEST_USER;Password=Password123;", Dbms = "SqlServer" }
        );
    }
}
