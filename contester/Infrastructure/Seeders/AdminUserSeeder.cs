using contester.Features.Authentication;
using contester.Features.Users;
using contester.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace contester.Infrastructure.Seeders;

public interface IAdminUserSeeder
{
    public Task Seed();
}

public class AdminUserSeeder(ApplicationDbContext context, IConfigurationReaderService configuration, ILogger<AdminUserSeeder> logger) : IAdminUserSeeder
{
    private async Task<UserRole> GetUserRole(string roleName)
    {
        var role = await context.UserRoles.AsNoTracking()
            .Where(ur => ur.Name == roleName)
            .FirstOrDefaultAsync();
        
        if (role == default)
            throw new ApplicationException("Role could not be found");
        
        return role;
    }
    
    private async Task RemoveAllAdmins()
    {
        logger.LogInformation("Removing all admins");

        var userRoleId = (await GetUserRole("User")).Id;
        var adminRoleId = (await GetUserRole("Admin")).Id;
        
        await context.Users
            .Where(u => u.UserRoleId == adminRoleId)
            .ExecuteUpdateAsync(u =>
                u.SetProperty(up => up.UserRoleId, up => userRoleId)
            );
        await context.SaveChangesAsync();
    }
    
    private async Task CreateAdminUser()
    {
        logger.LogInformation($"Creating a new admin user {configuration.GetAdminUserEmail()}");
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = configuration.GetAdminUserEmail(),
            FirstName = "Admin",
            LastName = "",
            AdditionalInfo = "",
            PasswordHash = "",
            UserRoleId = (await GetUserRole("Admin")).Id,
            EmailConfirmationCode = "",
            IsEmailConfirmed = true,
        };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, configuration.GetAdminUserPassword());

        await context.AddAsync(user);
        await context.SaveChangesAsync();
    }

    private async Task UpdateExistingUser(User user)
    {
        logger.LogInformation($"Setting user {user.Email} to be admin");
        
        user.UserRoleId = (await GetUserRole("Admin")).Id;
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, configuration.GetAdminUserPassword());

        await context.SaveChangesAsync();
    }
    
    public async Task Seed()
    {
        var user = await context.Users
            .Where(u => u.Email == configuration.GetAdminUserEmail())
            .FirstOrDefaultAsync();

        if (user is null)
        {
            logger.LogInformation($"User {configuration.GetAdminUserEmail()} does not exist, creating");
            await RemoveAllAdmins();
            await CreateAdminUser();
            return;
        }

        if (user!.UserRoleId != (await GetUserRole("Admin")).Id)
        {
            logger.LogInformation($"User {configuration.GetAdminUserEmail()} exists, setting to be admin");
            await RemoveAllAdmins();
            await UpdateExistingUser(user);
        }
    }
}
