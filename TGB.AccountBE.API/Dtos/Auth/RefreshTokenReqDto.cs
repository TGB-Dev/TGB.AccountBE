using System.ComponentModel.DataAnnotations;

namespace TGB.AccountBE.API.Dtos.Auth;

public class RefreshTokenReqDto
{
    [Required] public required string RefreshToken { get; set; }
}
