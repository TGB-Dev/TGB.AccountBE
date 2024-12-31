using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using TGB.AccountBE.API.Database;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
        // Don't know if we should keep this because the identity entities are managed by OpenIddict
        // and ASP.NET Core Identity, not us
        // o => o.UseNodaTime()
    );

    // Use OpenIddict entities
    options.UseOpenIddict();
});

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;

        // User settings
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization();

// OpenIddict offers native integration with Quartz.NET to perform scheduled tasks
// (like pruning orphaned authorizations/tokens from the database) at regular intervals.
builder.Services.AddQuartz(options =>
{
    options.UseSimpleTypeLoader();
    options.UseInMemoryStore();
});

// Register the Quartz.NET service and configure it to block shutdown until jobs are complete.
builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        // Share the same DbContext with ASP.NET Core Identity
        options.UseEntityFrameworkCore().UseDbContext<ApplicationDbContext>();

        // Use Quartz
        options.UseQuartz();
    })
    .AddClient(options =>
    {
        options.AllowAuthorizationCodeFlow();

        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();

        options.UseAspNetCore()
            .EnableStatusCodePagesIntegration()
            .EnableRedirectionEndpointPassthrough();

        options.UseSystemNetHttp()
            .SetProductInformation(typeof(Program).Assembly);

        options.UseWebProviders()
            .AddGoogle(options =>
            {
                var googleConfig = builder.Configuration
                    .GetRequiredSection("External")
                    .GetRequiredSection("Google");
                options
                    .SetClientId(googleConfig.GetValue<string>("ClientId") ?? string.Empty)
                    .SetClientSecret(googleConfig.GetValue<string>("ClientSecret") ?? string.Empty)
                    .SetRedirectUri(googleConfig.GetValue<string>("RedirectUri") ?? string.Empty);
            })
            .AddGitHub(options =>
            {
                var githubConfig = builder.Configuration
                    .GetRequiredSection("External")
                    .GetRequiredSection("GitHub");
                options
                    .SetClientId(githubConfig.GetValue<string>("ClientId") ?? string.Empty)
                    .SetClientSecret(githubConfig.GetValue<string>("ClientSecret") ?? string.Empty)
                    .SetRedirectUri(githubConfig.GetValue<string>("RedirectUri") ?? string.Empty);
            });
    })
    .AddServer(options =>
    {
        // Enable the authorization, logout, token and userinfo endpoints.
        options.SetAuthorizationEndpointUris("connect/authorize")
            .SetEndSessionEndpointUris("connect/logout")
            .SetTokenEndpointUris("connect/token")
            .SetUserInfoEndpointUris("connect/userinfo");

        options.RegisterScopes(
            Scopes.Email,
            Scopes.Profile,
            Scopes.Roles,
            Scopes.Phone
        );

        // Enable the authorization code flow.
        options.AllowAuthorizationCodeFlow();

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
    .AddValidation(options =>
    {
        // Import the configuration from the local OpenIddict server instance.
        options.UseLocalServer();

        // Register the ASP.NET Core host.
        options.UseAspNetCore();
    });
;

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseCors(options =>
{
    options
        .AllowAnyMethod()
        .WithOrigins(
            builder.Configuration.GetSection("CorsOrigins").Get<string[]>() ??
            ["http://localhost:4200"]
        );
});

app.UseAuthorization();
app.UseAuthentication();

app.MapControllers();
app.MapDefaultControllerRoute();

app.Run();
