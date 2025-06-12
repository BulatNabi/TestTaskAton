using Microsoft.AspNetCore.Identity;
using TestTaskAton.Models; 
namespace TestTaskAton.Data;
public class DbInitializer
{
    
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var logger = serviceProvider.GetRequiredService<ILogger<DbInitializer>>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        var adminLogin = configuration["Admin:Login"] ?? "Default"; 
        var adminPassword = configuration["Admin:Password"] ?? "Default";
        var adminName = configuration["Admin:Name"] ?? "Default";

        var adminUser = await userManager.FindByNameAsync(adminLogin);

        if (adminUser == null)
        {
            logger.LogInformation($"Creating admin user: {adminLogin}");
            adminUser = new User
            {
                Login = adminLogin,
                UserName = adminLogin, 
                Name = adminName,
                Gender = 1, 
                Birthday = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Admin = true,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "System",
                ModifiedOn = DateTime.UtcNow,
                ModifiedBy = "System"
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);

            if (createResult.Succeeded)
            {
                logger.LogInformation($"Successfully created admin user: {adminLogin}");
                var roleAssignResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                if (roleAssignResult.Succeeded)
                        logger.LogInformation($"Successfully assigned Admin role to {adminLogin}");
                else
                    logger.LogError($"Error assigning Admin role to {adminLogin}: {string.Join(", ", roleAssignResult.Errors.Select(e => e.Description))}");
            }
            else
                logger.LogError($"Error creating admin user {adminLogin}: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
        }
        else
        {
            logger.LogInformation($"Admin user '{adminLogin}' already exists.");
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                logger.LogInformation($"Assigning Admin role to existing admin user {adminLogin}");
                var roleAssignResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                if (!roleAssignResult.Succeeded)
                    logger.LogError($"Error assigning Admin role to existing admin user {adminLogin}: {string.Join(", ", roleAssignResult.Errors.Select(e => e.Description))}");
            }
        }
    }
}
