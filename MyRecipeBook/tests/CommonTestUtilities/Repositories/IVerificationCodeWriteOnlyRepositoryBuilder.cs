namespace CommonTestUtilities.Repositories;

using Moq;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Repositories.VerificationCode;

public class IVerificationCodeWriteOnlyRepositoryBuilder
{
    public static IVerificationCodeWriteOnlyRepository Build(int deletedRows = 1)
    {
        var mock = new Mock<IVerificationCodeWriteOnlyRepository>();

        mock.Setup(repository => repository.Delete(It.IsAny<VerificationCode>())).ReturnsAsync(deletedRows);

        return mock.Object;
    }
}
