namespace MvApplication.Models;

public record User
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string UserName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? PhoneNumber { get; init; }
    public UserRole Role { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? SecurityStamp { get; set; }
}

public enum UserRole
{
    Admin,
    User
}
