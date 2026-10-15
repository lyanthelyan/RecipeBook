using Moq;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Storage;

namespace CommonTestUtilities.Storage;

public class IStorageServiceBuilder
{
    public const string FakeUrl = "https://example.com/default-image.jpg";

    public static IStorageService Build()
    {
        var mock = new Mock<IStorageService>();

        mock.Setup(service => service.GetProfilePictureUrl(
            It.IsAny<User>())).Returns(FakeUrl);
        
        mock.Setup(service => service.GetRecipeIllustrationUrl(
            It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(FakeUrl);
        
        return mock.Object;
    }
}
