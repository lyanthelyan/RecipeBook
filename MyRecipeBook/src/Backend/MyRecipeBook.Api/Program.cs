using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using MyRecipeBook.Api.Filters;
using System.Globalization;
using MyRecipeBook.Application;
using MyRecipeBook.Infrastructure;
using MyRecipeBook.Api.Converters;
using Microsoft.EntityFrameworkCore.Storage;
using MyRecipeBook.Infrastructure.Migrations;

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
