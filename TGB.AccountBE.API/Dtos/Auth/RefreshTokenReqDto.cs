namespace TGB.AccountBE.API.Dtos.Auth;

public record RefreshTokenReqDto
{
    public required string RefreshToken { get; set; }
}
