using System.ComponentModel.DataAnnotations;
using TGB.AccountBE.API.Constants;

namespace TGB.AccountBE.API.Dtos.Auth;

public class LoginReqDto
{
    [Required]
    [MinLength(Constants.AuthRules.MIN_USERNAME_LENGTH)]
    [MaxLength(Constants.AuthRules.MAX_USERNAME_LENGTH)]
    [RegularExpression(Constants.AuthRules.USERNAME_PATTERN,
        ErrorMessage = Constants.AuthRules.USERNAME_ERROR_MESSAGE)]
    public required string UserName { get; set; }

    [Required]
    [MinLength(Constants.AuthRules.MIN_PASSWORD_LENGTH)]
    [RegularExpression(Constants.AuthRules.PASSWORD_PATTERN,
        ErrorMessage = Constants.AuthRules.PASSWORD_ERROR_MESSAGE)]
    public required string Password { get; set; }
}
