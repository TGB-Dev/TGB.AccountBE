namespace TGB.AccountBE.API.Dtos.User;

public class MeResDto
{
    public required string? UserName { get; set; }
    public required string? Email { get; set; }
    public required string? DisplayName { get; set; }
    public required DateTime? DateOfBirth { get; set; }
}
