using MyRecipeBook.Domain.Enums;

namespace MyRecipeBook.Domain.Repositories.VerificationCode;

public interface IVerificationCodeReadOnlyRepository
{
    Task<Entities.VerificationCode?> Get(Guid userId, string code, VerificationCodeType type);
}
