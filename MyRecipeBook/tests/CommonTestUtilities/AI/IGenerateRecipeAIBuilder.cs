using Moq;
using MyRecipeBook.Domain.AI;
using MyRecipeBook.Domain.Extensions;

namespace CommonTestUtilities.AI;

public class IGenerateRecipeAIBuilder
{
    public static IGenerateRecipeAI Build()
    {
        var mock = new Mock<IGenerateRecipeAI>();

        // An empty prompt simulates "not a recipe" (the model would return NO_RECIPE => null).
        // Any prompt with content returns a canonical recipe.
        mock.Setup(service => service.Generate(It.IsAny<string>()))
            .ReturnsAsync((string prompt) =>
            {
                return prompt.IsEmpty() ? null : GeneratedRecipeDtoBuilder.Build();
            });

        return mock.Object;
    }
}
