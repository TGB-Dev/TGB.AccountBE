using System.ComponentModel.DataAnnotations;
using TGB.AccountBE.API.Constants;

namespace TGB.AccountBE.API.Dtos.Auth;

public class ExternalLoginReqDto
{
    [Required] public required string DisplayName { get; set; }

    [Required]
    [MinLength(6)]
    [MaxLength(32)]
    [RegularExpression(Constants.AuthRules.USERNAME_PATTERN,
        ErrorMessage = Constants.AuthRules.USERNAME_ERROR_MESSAGE)]
    public required string UserName { get; set; }

    [Required] [EmailAddress] public required string Email { get; set; }
    [Required] public required DateTimeOffset DateOfBirth { get; set; }
}
