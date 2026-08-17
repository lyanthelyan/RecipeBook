using MyRecipeBook.Communication.Responses;

namespace MyRecipeBook.Application.UseCases.Recipe.GetRecent;

public interface IGetRecentRecipesUseCase
{
    Task<ResponseRecipesJson> Execute();
}
