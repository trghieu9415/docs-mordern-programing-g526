using System.ComponentModel.DataAnnotations;

namespace MvInfrastructure.Options;

public class RedisOptions {
  public static string SectionName => "Redis";

  [Required(ErrorMessage = "Redis Connection String là bắt buộc!")]
  public string Configuration { get; set; } = "localhost:6379";

  public string InstanceName { get; set; } = "MvProduct_";
}
