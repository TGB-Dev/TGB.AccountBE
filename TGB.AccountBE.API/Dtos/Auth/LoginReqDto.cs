using System.ComponentModel.DataAnnotations;
using TGB.AccountBE.API.Constants;

namespace TGB.AccountBE.API.Dtos.Auth;

public class LoginReqDto
{
    [Required]
    [MinLength(AuthRules.MIN_USERNAME_LENGTH)]
    [MaxLength(AuthRules.MAX_USERNAME_LENGTH)]
    [RegularExpression(AuthRules.USERNAME_PATTERN,
        ErrorMessage = AuthRules.USERNAME_ERROR_MESSAGE)]
    public required string UserName { get; set; }

    [Required]
    [MinLength(AuthRules.MIN_PASSWORD_LENGTH)]
    [RegularExpression(AuthRules.PASSWORD_PATTERN,
        ErrorMessage = AuthRules.PASSWORD_ERROR_MESSAGE)]
    public required string Password { get; set; }
}
