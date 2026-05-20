var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddSwaggerGen(); // Swagger Configuration
builder.Services.AddOpenApi();

var app = builder.Build();


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
