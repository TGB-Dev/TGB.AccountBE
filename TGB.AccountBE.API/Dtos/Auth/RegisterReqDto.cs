using System.ComponentModel.DataAnnotations;
using TGB.AccountBE.API.Constants;

namespace TGB.AccountBE.API.Dtos.Auth;

public record RegisterReqDto
{
    [Required] public required string DisplayName { get; set; }

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

    [Required] [EmailAddress] public required string Email { get; set; }
    [Required] public required DateTimeOffset DateOfBirth { get; set; }
}
