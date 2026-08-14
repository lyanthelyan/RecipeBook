namespace CommonTestUtilities.Repositories;

using Moq;
using MyRecipeBook.Domain.Repositories.VerificationCode;

public class IVerificationCodeWriteOnlyRepositoryBuilder
{
    public static IVerificationCodeWriteOnlyRepository Build()
    {
        var mock = new Mock<IVerificationCodeWriteOnlyRepository>();
        return mock.Object;
    }
}
