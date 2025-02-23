namespace AuthService.Infrastructure.Exceptions;

public class HighTimeException : Exception
{
    public HighTimeException(string message) : base(message)
    {
    }

    public HighTimeException(string message, Exception innerException) : base(message, innerException)
    {
    }
}