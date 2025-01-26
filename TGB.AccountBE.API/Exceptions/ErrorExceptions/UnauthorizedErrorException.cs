namespace TGB.AccountBE.API.Exceptions.ErrorExceptions;

public class UnauthorizedErrorException : AppException
{
    public UnauthorizedErrorException(string error, string message) : base(error, message,
        StatusCodes.Status401Unauthorized)
    {
    }
}
