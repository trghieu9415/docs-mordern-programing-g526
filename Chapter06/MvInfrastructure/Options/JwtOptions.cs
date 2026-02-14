using System.ComponentModel.DataAnnotations;

namespace MvInfrastructure.Options;

public class JwtOptions
{
    [Required(ErrorMessage = "Secret Key là bắt buộc!")]
    [MinLength(32, ErrorMessage = "Secret Key phải có ít nhất 32 ký tự")]
    public string SecretKey { get; set; } = null!;

    [Required]
    public string Issuer { get; set; } = null!;

    [Required]
    public string Audience { get; set; } = null!;

    [Range(1, 1440)] // 1 phút đến 24 giờ
    public int AccessTokenExpiryMinutes { get; set; } = 30;

    [Range(1, 10080)] // 1 phút đến 7 ngày
    public int RefreshTokenExpiryMinutes { get; set; } = 1440; // 24 giờ

    public static string SectionName => "Jwt";
}
