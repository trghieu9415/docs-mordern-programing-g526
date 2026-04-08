using System.ComponentModel.DataAnnotations;

namespace MvApplication.Configs.Options;

public class RedisOptions : IOptionSection {
  [Required(ErrorMessage = "Redis Connection String là bắt buộc!")]
  public string Configuration { get; set; } = "localhost:6379";

  public string InstanceName { get; set; } = "Cinema_";
  public static string SectionName => "Redis";
}
