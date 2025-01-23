using System.ComponentModel.DataAnnotations;

namespace TGB.AccountBE.API.Dtos.Auth;

public class LoginReqDto
{
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
}
