using System.ComponentModel.DataAnnotations;
using TGB.AccountBE.API.Constants;

namespace TGB.AccountBE.API.Dtos.Auth;

public class RegisterReqDto
{
    [Required] public required string DisplayName { get; set; }

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

    [Required] [EmailAddress] public required string Email { get; set; }
    [Required] public required DateTimeOffset DateOfBirth { get; set; }
}
