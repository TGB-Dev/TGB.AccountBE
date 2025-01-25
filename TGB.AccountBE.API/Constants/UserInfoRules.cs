namespace TGB.AccountBE.API.Constants;

public static class UserInfoRules
{
    public const string USERNAME_PATTERN = @"^[a-zA-Z0-9]+$";
    public const string USERNAME_DISALLOWED_CHARS_PATTERN = @"[^a-zA-Z0-9]";
    public const string USERNAME_ERROR_MESSAGE = "UserName must contain only letters and numbers";
    public const string PASSWORD_PATTERN = @"^(?=.*[A-Za-z])(?=.*\d).+$";
    public const string PASSWORD_ERROR_MESSAGE = "Password must contain at least one letter and one number";
}
