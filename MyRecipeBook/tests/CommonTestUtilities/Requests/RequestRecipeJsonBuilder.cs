using Bogus;
using MyRecipeBook.Communication.Enums;
using MyRecipeBook.Communication.Requests;

namespace CommonTestUtilities.Requests;

public class RequestRecipeJsonBuilder
{
    public static RequestRecipeJson Build()
    {
        var instructionOrder = 1;

        return new Faker<RequestRecipeJson>()
            .RuleFor(request => request.Title, faker => faker.Lorem.Word())
            .RuleFor(request => request.CookTime, faker => faker.PickRandom<CookTime>())
            .RuleFor(request => request.Ingredients,
                faker => faker.Make(3, () => faker.Commerce.ProductName()))
            .RuleFor(request => request.DishTypes,
                faker => faker.Make(2, () => faker.PickRandom<DishType>())
                    .Distinct()
                    .ToList())
            .RuleFor(request => request.Instructions,
                faker => faker.Make(3, () => new RequestRecipeInstructionJson
                {
                    Order = instructionOrder++,
                    Description = faker.Lorem.Sentence()
                }));
    }
}
