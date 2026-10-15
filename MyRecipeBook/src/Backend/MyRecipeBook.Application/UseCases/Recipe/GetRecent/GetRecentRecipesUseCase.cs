using Mapster;
using MyRecipeBook.Application.Extensions;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Identity;
using MyRecipeBook.Domain.Repositories.Recipe;
using MyRecipeBook.Domain.Storage;
namespace MyRecipeBook.Application.UseCases.Recipe.GetRecent;

public class GetRecentRecipesUseCase : IGetRecentRecipesUseCase
{
    private readonly ILoggedUser _loggedUser;
    private readonly IRecipeReadOnlyRepository _repository;
    private readonly IStorageService _storageService;

    public GetRecentRecipesUseCase(
        ILoggedUser loggedUser, 
        IRecipeReadOnlyRepository repository,
        IStorageService storageService)
    {
        _loggedUser = loggedUser;
        _repository = repository;
        _storageService = storageService;
    }

    public async Task<ResponseRecipesJson> Execute()
    {
        var userId = _loggedUser.GetUserId();
        var recipes = await _repository.GetRecentRecipes(userId);

        var response = new ResponseRecipesJson
        {
            Recipes = recipes.ToResponseJson(userId, _storageService)
        };
        
        return response;
    }
}
