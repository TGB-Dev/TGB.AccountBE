namespace TGB.AccountBE.API.Exceptions.ErrorExceptions;

public class BadRequestErrorException : AppException
{
    public BadRequestErrorException(string error, string message) : base(error,
        message, StatusCodes.Status400BadRequest)
    {
    }
}
