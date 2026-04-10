using MvApplication.Exceptions;
using MvApplication.Models;
using MvInfrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace MvInfrastructure.Seed;

public static class IdentityDataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await EnsureUserAsync(
          userManager,
          userName: "admin",
          email: "admin@example.com",
          password: "Admin@123",
          role: UserRole.Admin
        );

        await EnsureUserAsync(
          userManager,
          userName: "testuser",
          email: "user@example.com",
          password: "User@123",
          role: UserRole.User
        );
    }

    private static async Task EnsureUserAsync(
      UserManager<ApplicationUser> userManager,
      string userName,
      string email,
      string password,
      UserRole role
    )
    {
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            var shouldUpdate = false;

            if (existingUser.Role != role)
            {
                existingUser.Role = role;
                shouldUpdate = true;
            }

            if (!existingUser.LockoutEnabled)
            {
                existingUser.LockoutEnabled = true;
                shouldUpdate = true;
            }

            if (!existingUser.EmailConfirmed)
            {
                existingUser.EmailConfirmed = true;
                shouldUpdate = true;
            }

            if (shouldUpdate)
            {
                var updateResult = await userManager.UpdateAsync(existingUser);
                if (!updateResult.Succeeded)
                {
                    var errors = string.Join(", ", updateResult.Errors.Select(x => x.Description));
                    throw new AppException($"Không thể cập nhật seed user {email}: {errors}");
                }
            }

            return;
        }

        var user = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = true,
            Role = role,
            LockoutEnabled = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(x => x.Description));
            throw new AppException($"Không thể seed user {email}: {errors}");
        }
    }
}
