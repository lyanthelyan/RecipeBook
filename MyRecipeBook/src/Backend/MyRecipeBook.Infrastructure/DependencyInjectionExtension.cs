using Azure.Storage.Blobs;
using FluentMigrator.Runner;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyRecipeBook.Domain.AI;
using MyRecipeBook.Domain.Identity;
using MyRecipeBook.Domain.Repositories;
using MyRecipeBook.Domain.Repositories.Recipe;
using MyRecipeBook.Domain.Repositories.User;
using MyRecipeBook.Domain.Repositories.VerificationCode;
using MyRecipeBook.Domain.Security.PasswordHashing;
using MyRecipeBook.Domain.Security.Tokens;
using MyRecipeBook.Domain.Storage;
using MyRecipeBook.Infrastructure.AI;
using MyRecipeBook.Infrastructure.DataAcess;
using MyRecipeBook.Infrastructure.DataAcess.Repositories;
using MyRecipeBook.Infrastructure.Identity;
using MyRecipeBook.Infrastructure.Security.PasswordHashing;
using MyRecipeBook.Infrastructure.Security.Tokens;
using MyRecipeBook.Infrastructure.Storage;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Images;
using System.ClientModel;
using System.Reflection;

namespace MyRecipeBook.Infrastructure;

public static class DependencyInjectionExtension
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure(IConfiguration configuration)
        {
            services.AddRepositories();
            services.AddTokensHandlers(configuration);
            services.AddOpenAi(configuration);
            services.AddScoped<IPasswordHasher, Argon2PasswordHasher>();                             
            services.AddScoped<ILoggedUser, LoggedUser>();
            //services.AddSingleton(_ =>
            //{
            //    var connectionString = configuration.GetConnectionString("BlobStorage")!;
            //    return new BlobServiceClient(connectionString);
            //});
            services.AddScoped<IStorageService, DisabledStorageService>();
            services.AddFluentMigratorCore().ConfigureRunner(config =>
            {
                config
                    .AddSqlServer()
                    .WithGlobalConnectionString(_=> 
                    {
                        var connectionString = configuration.GetConnectionString("DefaultConnection");
                        return connectionString;
                    })
                    .ScanIn(Assembly.Load("MyRecipeBook.Infrastructure"))
                    .For.All();
            }).AddLogging(lb => lb.AddFluentMigratorConsole());
            services.AddDbContext<MyRecipeBookDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });            
            return services;
        }
        
        private void AddRepositories()
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IUserWriteOnlyRepository, UserRepository>();
            services.AddScoped<IUserReadOnlyRepository, UserRepository>();
            services.AddScoped<IUserUpdateOnlyRepository, UserRepository>();

            services.AddScoped<IRecipeWriteOnlyRepository, RecipeRepository>();
            services.AddScoped<IRecipeReadOnlyRepository, RecipeRepository>();
            services.AddScoped<IRecipeUpdateOnlyRepository, RecipeRepository>();
            services.AddScoped<IVerificationCodeWriteOnlyRepository, VerificationCodeRepository>();
            services.AddScoped<IVerificationCodeReadOnlyRepository, VerificationCodeRepository>();
            
        }
        private void AddTokensHandlers(IConfiguration configuration)
        {
            services.AddScoped<IAccessTokensGenerator>(provider =>
            {
                var expirationTimeInMinutes = configuration.GetValue<uint>("Jwt:ExpirationTimeMinutes");
                var signingkey = configuration.GetValue<string>("Jwt:SigningKey")!;
                return new JwtTokenHandler(expirationTimeInMinutes, signingkey);
            });
        }

        private void AddOpenAi(IConfiguration configuration)
        {
            services.AddSingleton(_ =>
            {
                var endpoint = configuration.GetValue<String>("Settings:OpenAI:EndPoint")!;
                var deploymentName = configuration.GetValue<String>("Settings:OpenAI:Chat:DeploymentName")!;
                var apiKey = configuration.GetValue<String>("Settings:OpenAI:ApiKey")!;
                
                return new ChatClient(
                    model: deploymentName, 
                    credential: new ApiKeyCredential(apiKey), 
                    options : new OpenAIClientOptions 
                    {
                        Endpoint = new Uri(endpoint)
                    });
            });
            services.AddSingleton(_ =>
            {
                var endpoint = configuration.GetValue<String>("Settings:OpenAI:EndPoint")!;
                var deploymentName = configuration.GetValue<String>("Settings:OpenAI:Image:DeploymentName")!;
                var apiKey = configuration.GetValue<String>("Settings:OpenAI:ApiKey")!;
                
                return new ImageClient(
                    model: deploymentName,
                    credential: new ApiKeyCredential(apiKey),
                    options: new OpenAIClientOptions
                    {
                        Endpoint = new Uri(endpoint)
                    });
            });
            services.AddScoped<IGenerateRecipeAI, ChatGptService>();
        }
    }
}
