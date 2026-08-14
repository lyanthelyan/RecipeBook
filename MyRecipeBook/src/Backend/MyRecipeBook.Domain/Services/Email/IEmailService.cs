namespace MyRecipeBook.Domain.Services.Email;

public interface IEmailService
{
    Task SendPasswordRecoveryCode(string email, string code);
}
