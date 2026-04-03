namespace MvApplication.Configs.Options;

public class RabbitMqOptions : IOptionSection {
  public string Host { get; set; } = "localhost";
  public string VirtualHost { get; set; } = "/";
  public string Username { get; set; } = "guest";
  public string Password { get; set; } = "guest";
  public static string SectionName => "RabbitMQ";
}
