using MvInfrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MvInfrastructure.Persistence;

public class AppDbContext : IdentityUserContext<AppUser, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
      : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Cấu hình bảng Users
        builder.Entity<AppUser>(entity =>
        {
            entity.ToTable("Users");
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Role).HasConversion<string>();
        });

        // Seed default admin user
        var adminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var admin = new AppUser
        {
            Id = adminId,
            UserName = "admin",
            NormalizedUserName = "ADMIN",
            Email = "admin@example.com",
            NormalizedEmail = "ADMIN@EXAMPLE.COM",
            EmailConfirmed = true,
            SecurityStamp = "ADMIN-SECURITY-STAMP-FIXED",
            ConcurrencyStamp = "ADMIN-CONCURRENCY-STAMP-FIXED",
            Role = MvApplication.Models.UserRole.Admin,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        // Password: Admin@123
        admin.PasswordHash = "AQAAAAIAAYagAAAAEJ7fZVxVv8YLNvDv8xPQhQqL1dB5z0YV7+lG5x5R5Z5Z5Z5Z5Z5Z5Z5Z5Z5Z5Q==";

        builder.Entity<AppUser>().HasData(admin);
    }
}
