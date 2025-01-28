namespace TGB.AccountBE.API.Constants;

public static class HttpErrorResponses
{
    // Register errors
    public const string UserNameAlreadyExists = "UserName already exists";
    public const string EmailAlreadyExists = "Email already exists";

    // Login errors
    public const string InvalidUserNameOrPassword = "Invalid username or password";
    public const string EmailNotConfirmed = "Email not confirmed";

    // Authorization errors
    public const string InvalidAccessToken = "Invalid access token";

    // NotFound errors
    public const string UserNotFound = "User not found";
    public const string UserSessionNotFound = "User session not found";

    // RefreshToken errors
    public const string RefreshTokenExpired = "Refresh token expired";

    // OIDC/OAuth errors
    public const string OAuthProviderNotSupported = "OAuth provider is not supported";
    public const string OAuthRegistrationError = "Error while registering user via OAuth provider";
    public const string InvalidOAuthRegisteredUser = "Invalid registered user from OAuth provider";
    public const string OAuthClientIdNotProvided = "OAuth client ID not provided";
    public const string OidcInvalidConsentType = "OIDC consent type is invalid";
    public const string OidcUserNotAllowedToSignIn = "OIDC user is not allowed to sign in";
    public const string OidcInvalidToken = "OIDC token is invalid";
}
