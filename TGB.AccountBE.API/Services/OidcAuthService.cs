using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using TGB.AccountBE.API.Constants;
using TGB.AccountBE.API.Exceptions.ErrorExceptions;
using TGB.AccountBE.API.Extensions;
using TGB.AccountBE.API.Interfaces.Services;
using TGB.AccountBE.API.Models.Sql;

namespace TGB.AccountBE.API.Services;

public class OidcAuthService : IOidcAuthService
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public OidcAuthService(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        IOpenIddictScopeManager scopeManager,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _scopeManager = scopeManager;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public async Task<IActionResult> Accept()
    {
        throw new NotImplementedException();
    }

    public async Task<ClaimsPrincipal> Authorize(OpenIddictRequest request,
        string userId)
    {
        // Currently we directly accept the request because:
        // 1. We don't accept external client applications' requests
        // 2. If needed in the future, the accept/deny functionality will be handled by the front-end
        // team

        // If this function is called, the user is authorized to do this, so we don't need to check
        // that, and proceed to the client application authorization

        // The flow:
        // Check the consent type
        // Generate claims
        // Return the principal information back to OpenIddict client and let the frontend handles
        // them

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            throw new BadRequestErrorException(nameof(HttpErrorResponses.UserNotFound),
                HttpErrorResponses.UserNotFound);

        if (request.ClientId == null)
            throw new BadRequestErrorException(nameof(HttpErrorResponses.OAuthClientIdNotProvided),
                HttpErrorResponses.OAuthClientIdNotProvided);

        var application = await _applicationManager.FindByClientIdAsync(
            request.ClientId
        );

        if (application is null)
            throw new BadRequestErrorException(nameof(HttpErrorResponses.OidcInvalidApplication),
                HttpErrorResponses.OidcInvalidApplication);

        if (await _applicationManager.GetConsentTypeAsync(application) is not OpenIddictConstants
                .ConsentTypes.Explicit)
            throw new BadRequestErrorException(nameof(HttpErrorResponses.OidcInvalidConsentType),
                HttpErrorResponses.OidcInvalidConsentType);

        // Retrieve the permanent authorizations associated with the user and the calling client application.
        var authorizations = await _authorizationManager.FindAsync(
            await _userManager.GetUserIdAsync(user),
            await _applicationManager.GetIdAsync(application),
            OpenIddictConstants.Statuses.Valid,
            OpenIddictConstants.AuthorizationTypes.Permanent,
            request.GetScopes()).ToListAsync();

        // Create the claims-based identity that will be used by OpenIddict to generate tokens.
        var identity = new ClaimsIdentity(
            TokenValidationParameters.DefaultAuthenticationType,
            OpenIddictConstants.Claims.Name,
            OpenIddictConstants.Claims.Role);

        // Add the claims that will be persisted in the tokens.
        identity.SetClaim(OpenIddictConstants.Claims.Subject,
                await _userManager.GetUserIdAsync(user))
            .SetClaim(OpenIddictConstants.Claims.Email, await _userManager.GetEmailAsync(user))
            .SetClaim(OpenIddictConstants.Claims.Name,
                await _userManager.GetUserNameAsync(user))
            .SetClaim(OpenIddictConstants.Claims.PreferredUsername,
                await _userManager.GetUserNameAsync(user))
            .SetClaims(OpenIddictConstants.Claims.Role,
                [.. await _userManager.GetRolesAsync(user)]);

        // Note: in this sample, the granted scopes match the requested scope,
        // but you may want to allow the user to uncheck specific scopes.
        // For that, simply restrict the list of scopes before calling SetScopes.
        identity.SetScopes(request.GetScopes());
        identity.SetResources(await _scopeManager.ListResourcesAsync(identity.GetScopes())
            .ToListAsync());

        // Automatically create a permanent authorization to avoid requiring explicit consent
        // for future authorization or token requests containing the same scopes.
        var authorization = authorizations.LastOrDefault();
        authorization ??= await _authorizationManager.CreateAsync(
            identity,
            await _userManager.GetUserIdAsync(user),
            await _applicationManager.GetIdAsync(application) ?? throw new BadRequestErrorException(
                nameof(HttpErrorResponses.OidcInvalidApplication),
                HttpErrorResponses.OidcInvalidApplication),
            OpenIddictConstants.AuthorizationTypes.Permanent,
            identity.GetScopes());

        identity.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));
        identity.SetDestinations(GetDestinations);

        return new ClaimsPrincipal(identity);
    }

    public async Task<IActionResult> Deny()
    {
        throw new NotImplementedException();
    }

    public async Task<ClaimsPrincipal> Exchange(OpenIddictRequest request,
        string userId, ClaimsPrincipal principal)
    {
        if (!request.IsAuthorizationCodeGrantType() && !request.IsRefreshTokenGrantType())
            throw new BadRequestErrorException(nameof(HttpErrorResponses.OidcInvalidGrantType),
                HttpErrorResponses.OidcInvalidGrantType);

        // Retrieve the user profile corresponding to the authorization code/refresh token.
        var user =
            await _userManager.FindByIdAsync(userId);
        if (user is null)
            throw new BadRequestErrorException(nameof(HttpErrorResponses.UserNotFound),
                HttpErrorResponses.UserNotFound);

        // Ensure the user is still allowed to sign in.
        if (!await _signInManager.CanSignInAsync(user))
            throw new BadRequestErrorException(
                nameof(HttpErrorResponses.OidcUserNotAllowedToSignIn),
                HttpErrorResponses.OidcUserNotAllowedToSignIn);

        var identity = new ClaimsIdentity(principal.Claims,
            TokenValidationParameters.DefaultAuthenticationType,
            OpenIddictConstants.Claims.Name,
            OpenIddictConstants.Claims.Role);

        // Override the user claims present in the principal in case they
        // changed since the authorization code/refresh token was issued.
        identity.SetClaim(OpenIddictConstants.Claims.Subject,
                await _userManager.GetUserIdAsync(user))
            .SetClaim(OpenIddictConstants.Claims.Email, await _userManager.GetEmailAsync(user))
            .SetClaim(OpenIddictConstants.Claims.Name,
                await _userManager.GetUserNameAsync(user))
            .SetClaim(OpenIddictConstants.Claims.PreferredUsername,
                await _userManager.GetUserNameAsync(user))
            .SetClaims(OpenIddictConstants.Claims.Role,
                [.. await _userManager.GetRolesAsync(user)]);

        identity.SetDestinations(GetDestinations);

        return new ClaimsPrincipal(identity);
    }

    public async Task<IActionResult> Logout()
    {
        throw new NotImplementedException();
    }

    public async Task<IActionResult> UserInfo(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        throw new NotImplementedException();
    }

    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        // Note: by default, claims are NOT automatically included in the access and identity tokens.
        // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
        // whether they should be included in access tokens, in identity tokens or in both.

        if (claim.Subject is null)
            throw new BadRequestErrorException(nameof(HttpErrorResponses.InvalidClaimSubject),
                HttpErrorResponses.InvalidClaimSubject);

        switch (claim.Type)
        {
            case OpenIddictConstants.Claims.Name or OpenIddictConstants.Claims.PreferredUsername:
                yield return OpenIddictConstants.Destinations.AccessToken;

                if (claim.Subject.HasScope(OpenIddictConstants.Permissions.Scopes.Profile))
                    yield return OpenIddictConstants.Destinations.IdentityToken;

                yield break;

            case OpenIddictConstants.Claims.Email:
                yield return OpenIddictConstants.Destinations.AccessToken;

                if (claim.Subject.HasScope(OpenIddictConstants.Permissions.Scopes.Email))
                    yield return OpenIddictConstants.Destinations.IdentityToken;

                yield break;

            case OpenIddictConstants.Claims.Role:
                yield return OpenIddictConstants.Destinations.AccessToken;

                if (claim.Subject.HasScope(OpenIddictConstants.Permissions.Scopes.Roles))
                    yield return OpenIddictConstants.Destinations.IdentityToken;

                yield break;

            // Never include the security stamp in the access and identity tokens, as it's a secret value.
            case "AspNet.Identity.SecurityStamp": yield break;

            default:
                yield return OpenIddictConstants.Destinations.AccessToken;
                yield break;
        }
    }
}
