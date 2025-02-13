using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using Quartz;
using Redis.OM;
using TGB.AccountBE.API;
using TGB.AccountBE.API.Constants;
using TGB.AccountBE.API.Database;
using TGB.AccountBE.API.Exceptions;
using TGB.AccountBE.API.Interfaces.Repository.RedisOm;
using TGB.AccountBE.API.Interfaces.Repository.Sql;
using TGB.AccountBE.API.Interfaces.Services;
using TGB.AccountBE.API.Models.Sql;
using TGB.AccountBE.API.Repository.RedisOm;
using TGB.AccountBE.API.Repository.Sql;
using TGB.AccountBE.API.Services;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);

// Add database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("SqlConnection") ?? "",
        o => o.UseNodaTime()
    );

    // Use OpenIddict entities
    options.UseOpenIddict();
});

// Add RabbitMq services
builder.Services.AddSingleton<IConnection>(_ =>
{
    var factory = new ConnectionFactory
    {
        HostName = builder.Configuration["RabbitMq:Host"]!,
        Port = int.Parse(builder.Configuration["RabbitMq:Port"]!),
        UserName = builder.Configuration["RabbitMq:Username"]!,
        Password = builder.Configuration["RabbitMq:Password"]!
    };
    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});
builder.Services.AddHostedService<RabbitMqSetupHostedService>();

// Add Redis Om services
builder.Services.AddSingleton(
    new RedisConnectionProvider(
        builder.Configuration.GetConnectionString("RedisConnection") ?? ""));
builder.Services.AddHostedService<IndexCreationService>();

// Add Redis repositories
builder.Services
    .AddScoped<IUserSessionRepositoryRedisOm,
        UserSessionRepositoryRedisOm>();

// Add Sql repositories
builder.Services
    .AddScoped<IUserSessionRepositorySql,
        UserSessionRepositorySql>();

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = AuthRules.MIN_PASSWORD_LENGTH;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;

        // User settings
        options.User.RequireUniqueEmail = true;
        options.User.AllowedUserNameCharacters =
            AuthRules.ALLOWED_USERNAME_CHARS;

        // Signin settings
        options.SignIn.RequireConfirmedEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Authentication and Authorization with other middlewares
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        options.DefaultForbidScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignOutScheme =
            JwtBearerDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Token:AccessToken:Issuer"],
            ValidAudience = builder.Configuration["Token:AccessToken:Audience"],
            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Token:AccessToken:SigningKey"]!))
        };
    });

// Add controllers' services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserSessionService, UserSessionService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOidcAuthService, OidcAuthService>();
builder.Services.AddScoped<IRandomPasswordGenerator, RandomPasswordGenerator>();

// Add database initializer
builder.Services.AddHostedService<ApplicationDbInitializer>();

// Add problem details
builder.Services.AddProblemDetails();

// Add exception handler
builder.Services.AddExceptionHandler<AppExceptionHandler>();

// OpenIddict offers native integration with Quartz.NET to perform scheduled tasks
// (like pruning orphaned authorizations/tokens from the database) at regular intervals.
builder.Services.AddQuartz(options =>
{
    options.UseSimpleTypeLoader();
    options.UseInMemoryStore();
});

// Register the Quartz.NET service and configure it to block shutdown until jobs are complete.
builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

// We're using OpenIddict with the authorization code flow + PKCE
builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        // Share the same DbContext with ASP.NET Core Identity
        options.UseEntityFrameworkCore().UseDbContext<ApplicationDbContext>();

        // Use Quartz
        options.UseQuartz();
    })
    .AddServer(options =>
    {
        // Enable the authorization, logout, token and userinfo endpoints.
        options.SetAuthorizationEndpointUris("/api/Oidc/Authorize")
            .SetEndSessionEndpointUris("/api/Oidc/Logout")
            .SetTokenEndpointUris("/api/Oidc/Token")
            .SetUserInfoEndpointUris("/api/Oidc/UserInfo");

        options.RegisterScopes(
            Scopes.Email,
            Scopes.Profile,
            Scopes.Roles,
            Scopes.Phone
        );

        // Enable the authorization code flow with PKCE.
        options.AllowAuthorizationCodeFlow()
            .RequireProofKeyForCodeExchange();

        // Add the X.509 encryption certificate to ensure secure communication
        // This should be the same certificate used on the IdP
        var certificatePath = builder.Configuration["OpenIddict:X509CertPath"];
        if (certificatePath is null)
        {
            throw new InvalidConfigurationException("OpenIddict:X509CertPath is not configured.");
        }

        if (!Path.Exists(certificatePath))
        {
            throw new InvalidConfigurationException("OpenIddict:X509CertPath does not exist.");
        }

        var certificatePassword = builder.Configuration["OpenIddict:X509CertPassword"] ??
                                  throw new InvalidConfigurationException(
                                      "OpenIddict:X509CertPassword is not configured.");
        var certificateStream =
            File.Open(certificatePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        options.AddEncryptionCertificate(certificateStream, certificatePassword);

        // Register the signing and encryption credentials.
        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();

        // Register the ASP.NET Core host and configure the ASP.NET Core options.
        options.UseAspNetCore()
            .EnableAuthorizationEndpointPassthrough()
            .EnableEndSessionEndpointPassthrough()
            .EnableTokenEndpointPassthrough()
            .EnableUserInfoEndpointPassthrough()
            .EnableStatusCodePagesIntegration();
    })
    .AddClient(options =>
    {
        options.SetRedirectionEndpointUris("api/Auth/Token");
        options.AllowAuthorizationCodeFlow();

        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();

        options.UseAspNetCore()
            .EnableStatusCodePagesIntegration()
            .EnableRedirectionEndpointPassthrough();

        options.UseSystemNetHttp()
            .SetProductInformation(typeof(Program).Assembly);

        // Configure external authentication providers
        var externalAuthConfig = builder.Configuration.GetSection("ExternalAuth");
        if (!externalAuthConfig.Exists()) return;

        // Google
        var googleAuthConfig = externalAuthConfig.GetSection("Google");
        if (googleAuthConfig.Exists())
        {
            var clientId = googleAuthConfig["ClientId"]!;
            var clientSecret = googleAuthConfig["ClientSecret"]!;
            var redirectUri = googleAuthConfig["RedirectUri"]!;
            options.UseWebProviders().AddGoogle(authOptions =>
            {
                authOptions.SetClientId(clientId)
                    .SetClientSecret(clientSecret)
                    .SetRedirectUri(redirectUri)
                    .AddScopes(Scopes.Email, Scopes.Profile);
            });
        }

        // GitHub
        var githubAuthConfig = externalAuthConfig.GetSection("GitHub");
        if (githubAuthConfig.Exists())
        {
            var clientId = githubAuthConfig["ClientId"]!;
            var clientSecret = githubAuthConfig["ClientSecret"]!;
            var redirectUri = githubAuthConfig["RedirectUri"]!;
            options.UseWebProviders().AddGitHub(authOptions =>
            {
                authOptions.SetClientId(clientId)
                    .SetClientSecret(clientSecret)
                    .SetRedirectUri(redirectUri);
            });
        }
    })
    .AddValidation(options =>
    {
        // Import the configuration from the local OpenIddict server instance.
        options.UseLocalServer();

        // Register the ASP.NET Core host.
        options.UseAspNetCore();
    });

// Add controllers and generate Swagger/OpenAPI documentation
// Learn more at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description =
            "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add a worker to handle external application registration
// TODO: replace this in production with an administration web UI
builder.Services.AddHostedService<ApplicationRegisterWorkerHostedService>();
// Configure the HTTP request pipeline.
var app = builder.Build();

app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    // Don't use this because:
    // 1. The AppExceptionHandler is detailed enough
    // 2. This option is conflicting with the already existing AppExceptionHandler
    // app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseMigrationsEndPoint();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors(options =>
{
    if (app.Environment.IsDevelopment())
    {
        options.AllowAnyMethod().AllowAnyHeader().WithOrigins("*").WithHeaders("*");
    }
    else
    {
        var corsOrigins = builder.Configuration.GetSection("CorsOrigins").Get<string[]>();
        options
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithOrigins(
                corsOrigins ??
                ["http://localhost:4200"]
            );
    }
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapDefaultControllerRoute();

app.Run();
