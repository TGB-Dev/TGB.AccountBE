namespace TGB.AccountBE.API.Exceptions;

public class AppException : Exception
{
    public AppException(string message, string error, int statusCode) : base(message)
    {
        StatusCode = statusCode;
        Error = error;
    }

    public int StatusCode { get; set; }
    public string Error { get; set; }
}
