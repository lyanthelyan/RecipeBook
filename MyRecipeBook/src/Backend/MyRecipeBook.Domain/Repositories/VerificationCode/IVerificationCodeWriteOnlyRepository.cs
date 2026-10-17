namespace MyRecipeBook.Domain.Repositories.VerificationCode;

public interface IVerificationCodeWriteOnlyRepository
{
    Task Replace(Entities.VerificationCode verificationCode);
    Task<int> Delete(Entities.VerificationCode verificationCode);
}
