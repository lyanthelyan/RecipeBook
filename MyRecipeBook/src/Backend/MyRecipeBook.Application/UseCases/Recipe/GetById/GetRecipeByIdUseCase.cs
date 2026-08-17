using Mapster;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Identity;
using MyRecipeBook.Domain.Repositories.Recipe;
using MyRecipeBook.Exception;
using MyRecipeBook.Exception.ExceptionsBase;

namespace MyRecipeBook.Application.UseCases.Recipe.GetById;

public class GetRecipeByIdUseCase : IGetRecipeByIdUseCase
{
    private readonly ILoggedUser _loggedUser;
    private readonly IRecipeReadOnlyRepository _recipeRepository;
    public GetRecipeByIdUseCase(
        ILoggedUser loggedUser, 
        IRecipeReadOnlyRepository recipeRepository)
    {
        _loggedUser = loggedUser;
        _recipeRepository = recipeRepository;
    }
    public async Task<ResponseRecipeJson> Execute(Guid recipeId)
    {
        
        var recipe = await _recipeRepository.GetById(recipeId, _loggedUser.GetUserId());
        
        if (recipe is null)
            throw new NotFoundException(ResourceMessagesException.VALIDATION_RECIPE_NOT_FOUND);

        return recipe.Adapt<ResponseRecipeJson>();

    }
}
