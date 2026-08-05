
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MyRecipeBook.Api.Converters;
using MyRecipeBook.Api.Filters;
using MyRecipeBook.Api.Token;
using MyRecipeBook.Application;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Repositories.User;
using MyRecipeBook.Domain.Security.Tokens;
using MyRecipeBook.Exception;
using MyRecipeBook.Infrastructure;
using MyRecipeBook.Infrastructure.Migrations;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Converts strings received in JSON before they reach the endpoints.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new StringConverter()));

builder.Services.AddSwaggerGen(options =>
{
    // Adds JWT authentication support to Swagger.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter only your access token. Swagger will add 'Bearer' automatically.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    // Applies Bearer authentication to protected endpoints in Swagger.
    options.AddSecurityRequirement(openApiDocument =>
    {
        return new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference("Bearer", openApiDocument),
                []
            }
        };
    });
});

builder.Services.AddOpenApi();

// Registers Infrastructure and Application dependencies.
builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddApplication();

// Provides access to the token from the current HTTP request.
builder.Services.AddScoped<IAccessTokenProvider, HttpContextTokenProvider>();

// Allows services to access the current HttpContext.
builder.Services.AddHttpContextAccessor();

// Configures the supported request languages.
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new List<CultureInfo>
    {
        new CultureInfo("en"),
        new CultureInfo("pt-BR"),
        new CultureInfo("es")
    };

    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    // Gets the request language from the Accept-Language header.
    options.RequestCultureProviders =
    [
        new AcceptLanguageHeaderRequestCultureProvider()
    ];
});

// Registers the global exception filter.
builder.Services.AddMvc(options =>
    options.Filters.Add<ExceptionFilter>());

// Converts endpoint URLs to lowercase.
builder.Services.AddRouting(options =>
    options.LowercaseUrls = true);

// Configures JWT authentication.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(jwtOptions =>
    {
        var signingKey =
            builder.Configuration.GetValue<string>("Jwt:SigningKey")!;

        // Defines the rules used to validate access tokens.
        jwtOptions.TokenValidationParameters = new()
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(signingKey)),
            ClockSkew = TimeSpan.Zero
        };

        jwtOptions.Events = new JwtBearerEvents
        {
            // Checks whether the token belongs to an active user.
            OnTokenValidated = async context =>
            {
                var subject =
                    context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub)
                    ??
                    context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

                if (Guid.TryParse(subject, out var userId) is false)
                {
                    context.Fail("Invalid token subject");
                    return;
                }

                var userRepository = context.HttpContext.RequestServices
                    .GetRequiredService<IUserReadOnlyRepository>();

                var userExists =
                    await userRepository.ExistActiveUserWithId(userId);

                if (userExists is false)
                {
                    context.Fail("User not found or inactive");
                }
            },

            // Returns a custom JSON response when authentication fails.
            OnChallenge = async context =>
            {
                context.HandleResponse();

                context.Response.StatusCode =
                    StatusCodes.Status401Unauthorized;

                context.Response.ContentType = "application/json";

                var response = context.AuthenticateFailure switch
                {
                    null => new ResponseErrorJson(
                        ResourceMessagesException
                            .VALIDATION_ACCESS_TOKEN_REQUIRED),

                    SecurityTokenExpiredException =>
                        new ResponseErrorJson(
                            "Token Expired",
                            accessTokenExpired: true),

                    _ => new ResponseErrorJson(
                        ResourceMessagesException
                            .VALIDATION_RESOURCE_ACCESS_DENIED)
                };

                await context.Response.WriteAsJsonAsync(response);
            }
        };
    });

var app = builder.Build();

// Applies the configured localization rules to each request.
var localizationOptions =
    app.Services.GetRequiredService<
        IOptions<RequestLocalizationOptions>>();

app.UseRequestLocalization(localizationOptions.Value);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Executes pending database migrations before starting the API.
await ExecuteMigrations();

app.Run();

// Creates a dependency injection scope and runs the migrations.
async Task ExecuteMigrations()
{
    await using var scope = app.Services.CreateAsyncScope();

    DatabaseMigration.ExecuteMigrations(scope.ServiceProvider);
}

public partial class Program { }
