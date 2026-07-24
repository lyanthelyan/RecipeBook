using Bogus;
using Moq;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Security.Tokens;


namespace CommonTestUtilities.Security;

public class IAcessTokenGeneratorBuilder
{
    public static IAcessTokensGenerator Build()
    {
        var mock = new Mock<IAcessTokensGenerator>();

        var fakeToken = new Faker().Random.String2(32, "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789");

        mock.Setup(generator => generator.Generate(It.IsAny<User>())).Returns(fakeToken);

        return mock.Object;
    }
}
