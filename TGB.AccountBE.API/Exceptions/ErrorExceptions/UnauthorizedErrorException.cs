namespace TGB.AccountBE.API.Exceptions.ErrorExceptions;

public class UnauthorizedErrorException : AppException
{
    public UnauthorizedErrorException(string message, string error) : base(message, error,
        StatusCodes.Status401Unauthorized)
    {
    }
}
