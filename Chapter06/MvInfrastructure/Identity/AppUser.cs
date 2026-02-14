using System.ComponentModel.DataAnnotations;
using MvApplication.Models;
using Microsoft.AspNetCore.Identity;

namespace MvInfrastructure.Identity;

public class AppUser : IdentityUser<Guid>
{
    // Custom properties
    [MaxLength(100)]
    public string? FullName { get; set; }

    [Required]
    public UserRole Role { get; set; } = UserRole.User;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    // Navigation to application domain model
    public User ToUser()
    {
        return new User
        {
            Id = Id,
            UserName = UserName!,
            Email = Email!,
            PhoneNumber = PhoneNumber,
            Role = Role,
            CreatedAt = CreatedAt,
            SecurityStamp = SecurityStamp
        };
    }
}
