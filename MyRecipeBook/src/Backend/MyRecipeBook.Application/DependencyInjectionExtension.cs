using Microsoft.Extensions.DependencyInjection;
using MyRecipeBook.Application.UseCases.Login.WithEmailAndPassword;
using MyRecipeBook.Application.UseCases.User.Register;
using MyRecipeBook.Domain.Security.Tokens;

namespace MyRecipeBook.Application;

public static class DependencyInjectionExtension
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplication()
        {
            services.AddUseCases();
            return services;
        }
        private void AddUseCases()
        {
            services.AddScoped<IRegisterUserAccountUseCase, RegisterUserAccountUseCase>();
            services.AddScoped<ILoginWithEmailAndPasswordUseCase, LoginWithEmailAndPasswordUseCase>();
        }
    }
}
