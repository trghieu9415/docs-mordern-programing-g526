using Serilog;
using Serilog.Events;

namespace MvPresentation.Extensions;

public static class SerilogExtension {
  public static void AddSerilogCustom(this WebApplicationBuilder builder) {
    var logDir = Path.Combine(Directory.GetCurrentDirectory(), "logs");
    if (!Directory.Exists(logDir)) {
      Directory.CreateDirectory(logDir);
    }

    var businessInfoPath = Path.Combine(logDir, "biz-info-.log");
    var businessErrorPath = Path.Combine(logDir, "biz-error-.log");
    var sysWarnPath = Path.Combine(logDir, "sys-warn-.log");
    var sysErrorPath = Path.Combine(logDir, "sys-error-.log");

    const string sysTemplate =
      "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}";
    const string bizTemplate =
      "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}";

    Log.Logger = new LoggerConfiguration()
      .MinimumLevel.Information()
      .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
      .MinimumLevel.Override("System", LogEventLevel.Warning)
      .Enrich.FromLogContext()
      .WriteTo.Logger(logConfig => logConfig
        .Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Error)
        .WriteTo.File(
          sysErrorPath,
          rollingInterval: RollingInterval.Day,
          outputTemplate: sysTemplate))
      .WriteTo.Logger(logConfig => logConfig
        .Filter.ByIncludingOnly(e =>
          e.Level == LogEventLevel.Warning && !IsBusinessLog(e))
        .WriteTo.File(
          sysWarnPath,
          rollingInterval: RollingInterval.Day,
          outputTemplate: sysTemplate))
      .WriteTo.Logger(logConfig => logConfig
        .Filter.ByIncludingOnly(e =>
          e.Level == LogEventLevel.Warning && IsBusinessLog(e))
        .WriteTo.File(
          businessErrorPath,
          rollingInterval: RollingInterval.Day,
          outputTemplate: bizTemplate))
      .WriteTo.Logger(logConfig => logConfig
        .Filter.ByIncludingOnly(e =>
          e.Level == LogEventLevel.Information && IsBusinessLog(e))
        .WriteTo.File(
          businessInfoPath,
          rollingInterval: RollingInterval.Day,
          outputTemplate: bizTemplate))
      .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
      )
      .CreateLogger();

    builder.Host.UseSerilog();
  }

  private static bool IsBusinessLog(LogEvent logEvent) {
    if (!logEvent.Properties.TryGetValue("LogType", out var value)) {
      return false;
    }

    var logType = value.ToString().Trim();
    return logType is "Business" or "BusinessError" or "BusinessInfo";
  }
}
