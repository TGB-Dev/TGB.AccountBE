using System.ComponentModel.DataAnnotations;
using TGB.AccountBE.API.Constants;

namespace TGB.AccountBE.API.Dtos.Auth;

public class RegisterReqDto
{
    [Required] public required string DisplayName { get; set; }

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

    [Required] [EmailAddress] public required string Email { get; set; }
    [Required] public required DateTimeOffset DateOfBirth { get; set; }
}
