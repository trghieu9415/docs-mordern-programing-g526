namespace MvApplication.Exceptions;

public class ValidationException(IEnumerable<string> errors) : Exception("Invalid data") {
  public IEnumerable<string> Errors { get; } = errors;
}
