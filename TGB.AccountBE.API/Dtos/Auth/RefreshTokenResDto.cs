namespace TGB.AccountBE.API.Dtos.Auth;

public record RefreshTokenResDto
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}
