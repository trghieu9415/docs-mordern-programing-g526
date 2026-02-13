namespace MvApplication.Ports;

public interface IAppLogger<T> {
  void LogBusinessInformation(string message, params object[] args);
  void LogBusinessError(Exception ex, string message, params object[] args);
  void LogSystemWarning(string message, params object[] args);
  void LogSystemError(Exception ex, string message, params object[] args);
}
