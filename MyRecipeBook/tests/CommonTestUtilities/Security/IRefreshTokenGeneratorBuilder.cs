using Bogus;
using Moq;
using MyRecipeBook.Domain.Security.Tokens;

namespace CommonTestUtilities.Security;

public class IRefreshTokenGeneratorBuilder
{
    public static IRefreshTokenGenerator Build()
    {
        var mock = new Mock<IRefreshTokenGenerator>();

        var fakeToken = new Faker().Random.String2(32, "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789");

        mock.Setup(generator => generator.Generate()).Returns(fakeToken);

        return mock.Object;
    }
}
