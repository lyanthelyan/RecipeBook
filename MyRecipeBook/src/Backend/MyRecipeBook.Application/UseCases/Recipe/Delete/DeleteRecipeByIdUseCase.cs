using MyRecipeBook.Domain.Identity;
using MyRecipeBook.Domain.Repositories.Recipe;
using MyRecipeBook.Exception;
using MyRecipeBook.Exception.ExceptionsBase;

namespace MyRecipeBook.Application.UseCases.Recipe.Delete;

public class DeleteRecipeByIdUseCase : IDeleteRecipeByIdUseCase
{
    private readonly ILoggedUser _loggedUser;
    private readonly IRecipeWriteOnlyRepository _repository;

    public DeleteRecipeByIdUseCase(ILoggedUser loggedUser, IRecipeWriteOnlyRepository repository)
    {
        _loggedUser = loggedUser;
        _repository = repository;

    }
    public async Task Execute(Guid recipeId)
    {
        var deleted = await _repository.DeleteById(recipeId, _loggedUser.GetUserId());
        if (deleted is false)
            throw new NotFoundException(ResourceMessagesException.VALIDATION_RECIPE_NOT_FOUND);


    }
}
