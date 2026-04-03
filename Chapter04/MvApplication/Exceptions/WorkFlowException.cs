namespace MvApplication.Exceptions;

public class WorkFlowException(string message, int statusCode = 400) : Exception(message) {
  public int StatusCode { get; } = statusCode;
}
