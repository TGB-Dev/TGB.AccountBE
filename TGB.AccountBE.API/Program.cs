using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using Redis.OM;
using TGB.AccountBE.API.Database;
using TGB.AccountBE.API.Exceptions;
using TGB.AccountBE.API.Interfaces.Repository.RedisOm;
using TGB.AccountBE.API.Interfaces.Repository.Sql;
using TGB.AccountBE.API.Interfaces.Services;
using TGB.AccountBE.API.Models.Sql;
using TGB.AccountBE.API.Repository.RedisOm;
using TGB.AccountBE.API.Repository.Sql;
using TGB.AccountBE.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add controllers and generate Swagger/OpenAPI documentation

builder.Services.AddEndpointsApiExplorer();
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

builder.Services.AddControllers();

// Add database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("SqlConnection") ?? ""
    );
});

// Add RabbitMq services
builder.Services.AddSingleton<IConnection>(_ =>
{
    var factory = new ConnectionFactory
    {
        HostName = builder.Configuration["RabbitMq:Host"]!,
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
        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;

        // User settings
        options.User.RequireUniqueEmail = true;
        options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        // Signin settings
        options.SignIn.RequireConfirmedEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Authentication and Authorization with other middlewares
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
        options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultSignOutScheme =
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

builder.Services.AddAuthorization();

// Add controllers' services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserSessionService, UserSessionService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Add database initializer
builder.Services.AddHostedService<ApplicationDbInitializer>();

// Add problem details
builder.Services.AddProblemDetails();

// Add exception handler
builder.Services.AddExceptionHandler<AppExceptionHandler>();

// Configure the HTTP request pipeline.
var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
