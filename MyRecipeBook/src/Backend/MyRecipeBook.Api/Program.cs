using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyRecipeBook.Api.Converters;
using MyRecipeBook.Api.Filters;
using MyRecipeBook.Application;
using MyRecipeBook.Domain.Extensions;
using MyRecipeBook.Domain.Repositories.User;
using MyRecipeBook.Infrastructure;
using MyRecipeBook.Infrastructure.Migrations;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers() //String converter
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new StringConverter()));
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

//Dependency injection
builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddApplication();

//Localization configuration
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

    
    options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new AcceptLanguageHeaderRequestCultureProvider()
    };
});

//Global exception handling
builder.Services.AddMvc(options => options.Filters.Add<ExceptionFilter>());
 
builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(jwtoptions =>
    {
        var signingKey = builder.Configuration.GetValue<string>("Jwt:SigningKey")!;

        jwtoptions.TokenValidationParameters = new()
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ClockSkew = TimeSpan.Zero
        };

        jwtoptions.Events = new JwtBearerEvents 
        {
            OnTokenValidated = async context =>
            {
                var userId = context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ??
                context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId.IsEmpty())
                {
                    context.Fail("Invalid subject");
                    return;
                }

                var userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserReadOnlyRepository>();

                var existUser = await userRepository.ExistActiveUserWithId(Guid.Parse(userId));

                if (existUser is false)
                {
                    context.Fail("Invalid user");
                }
            }
        };
    });

var app = builder.Build();

//Localization middleware
var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
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

await ExecuteMigrations();

app.Run();

async Task ExecuteMigrations()
{
    await using var scope = app.Services.CreateAsyncScope();
    DatabaseMigration.ExecuteMigrations(scope.ServiceProvider);
}

public partial class Program{}
