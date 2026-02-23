namespace MvApplication.Exceptions;

public class ConcurrencyException(string message) : AppException(message, 409);
