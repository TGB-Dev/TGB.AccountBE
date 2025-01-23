using System.ComponentModel.DataAnnotations;

namespace TGB.AccountBE.API.Dtos.Auth;

public class RegisterReqDto
{
    [Required] public required string DisplayName { get; set; }

    [Required]
    [MinLength(6)]
    [MaxLength(32)]
    [RegularExpression(@"^[a-zA-Z0-9]+$",
        ErrorMessage = "UserName must contain only letters and numbers")]
    public required string UserName { get; set; }

    [Required]
    [MinLength(8)]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$",
        ErrorMessage = "Password must contain at least one letter and one number")]
    public required string Password { get; set; }

    [Required] [EmailAddress] public required string Email { get; set; }
    [Required] public required DateTimeOffset DateOfBirth { get; set; }
}
