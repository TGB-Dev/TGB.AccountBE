namespace TGB.AccountBE.API.Dtos.Auth;

public class LoginResDto
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}
