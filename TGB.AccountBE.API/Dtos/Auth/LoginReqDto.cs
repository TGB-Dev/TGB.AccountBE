using System.ComponentModel.DataAnnotations;
using TGB.AccountBE.API.Constants;

namespace TGB.AccountBE.API.Dtos.Auth;

public class LoginReqDto
{
    [Required]
    [MinLength(6)]
    [MaxLength(32)]
    [RegularExpression(UserInfoRules.USERNAME_PATTERN,
        ErrorMessage = UserInfoRules.USERNAME_ERROR_MESSAGE)]
    public required string UserName { get; set; }

    [Required]
    [MinLength(8)]
    [RegularExpression(UserInfoRules.PASSWORD_PATTERN,
        ErrorMessage = UserInfoRules.PASSWORD_ERROR_MESSAGE)]
    public required string Password { get; set; }
}
