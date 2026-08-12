using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Identity;
using MyRecipeBook.Domain.Repositories.Recipe;
using Mapster;
namespace MyRecipeBook.Application.UseCases.Recipe.GetRecent;

public class GetRecentRecipesUseCase : IGetRecentRecipesUseCase
{
    private readonly ILoggedUser _loggedUser;
    private readonly IRecipeReadOnlyRepository _repository;

    public GetRecentRecipesUseCase(ILoggedUser loggedUser, IRecipeReadOnlyRepository repository)
    {
        _loggedUser = loggedUser;
        _repository = repository;
    }

    public async Task<ResponseRecipesJson> Execute()
    {
        var recipes = await _repository.GetRecentRecipes(_loggedUser.GetUserId());

        var response = new ResponseRecipesJson
        {
            Recipes = recipes.Adapt<IList<ResponseRecipeSummaryJson>>()
        };
        
        return response;
    }
}
