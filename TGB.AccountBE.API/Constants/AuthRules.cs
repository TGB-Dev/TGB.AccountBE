namespace TGB.AccountBE.API.Constants;

public static class AuthRules
{
    public const string USERNAME_PATTERN = @"^[a-zA-Z0-9]+$";
    public const string USERNAME_DISALLOWED_CHARS_PATTERN = @"[^a-zA-Z0-9]";
    public const string USERNAME_ERROR_MESSAGE = "UserName must contain only letters and numbers";
    public const string PASSWORD_PATTERN = @"^(?=.*[A-Za-z])(?=.*\d).+$";

    public const string PASSWORD_ERROR_MESSAGE =
        "Password must contain at least one letter and one number";

    public const int MIN_USERNAME_LENGTH = 6;
    public const int MAX_USERNAME_LENGTH = 32;
    public const int USERNAME_RANDOM_PADDING = 4;
    public const int MIN_PASSWORD_LENGTH = 8;

    public static readonly string[] SUPPORTED_EXTERNAL_OAUTH_PROVIDERS = ["GitHub", "Google"];
}
