namespace TGB.AccountBE.API.Exceptions.ErrorExceptions;

public class BadRequestErrorException : AppException
{
    public BadRequestErrorException(string message, string error) : base(message,
        error, StatusCodes.Status400BadRequest)
    {
    }
}
