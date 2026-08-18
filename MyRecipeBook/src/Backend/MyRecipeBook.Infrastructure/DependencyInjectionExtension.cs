using FluentMigrator.Runner;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyRecipeBook.Domain.Identity;
using MyRecipeBook.Domain.Repositories;
using MyRecipeBook.Domain.Repositories.Recipe;
using MyRecipeBook.Domain.Repositories.User;
using MyRecipeBook.Domain.Repositories.VerificationCode;
using MyRecipeBook.Domain.Security.PasswordHashing;
using MyRecipeBook.Domain.Security.Tokens;
using MyRecipeBook.Infrastructure.DataAcess;
using MyRecipeBook.Infrastructure.DataAcess.Repositories;
using MyRecipeBook.Infrastructure.Identity;
using MyRecipeBook.Infrastructure.Security.PasswordHashing;
using MyRecipeBook.Infrastructure.Security.Tokens;
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
            services.AddScoped<IPasswordHasher, Argon2PasswordHasher>();       
            services.AddDbContext<MyRecipeBookDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });            
            
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
            services.AddScoped<ILoggedUser, LoggedUser>();
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
    }
}
