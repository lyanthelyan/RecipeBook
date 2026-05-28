using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddSwaggerGen(); // Swagger Configuration
builder.Services.AddOpenApi();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    // Supported languages
    var supportedCultures = new List<CultureInfo>
    {
        new CultureInfo("en"),
        new CultureInfo("pt-BR"),
        new CultureInfo("es")
    };

    // Default language
    options.DefaultRequestCulture = new RequestCulture("en");

    // Formatting cultures (dates, numbers, currency)
    options.SupportedCultures = supportedCultures;

    // UI/message cultures
    options.SupportedUICultures = supportedCultures;

    // Gets user language from browser
    options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new AcceptLanguageHeaderRequestCultureProvider()
    };
});



var app = builder.Build();

var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(localizationOptions.Value);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    app.UseSwagger(); //Swagger Configuration
    app.UseSwaggerUI(); //Swagger Configuration
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
