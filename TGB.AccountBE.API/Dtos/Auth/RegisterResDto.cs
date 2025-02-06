namespace TGB.AccountBE.API.Dtos.Auth;

public record RegisterResDto
{
    public required bool Succeeded { get; set; }
    public required string Message { get; set; }
}
