using MyRecipeBook.Domain.Identity;
using MyRecipeBook.Domain.Repositories.Recipe;
using MyRecipeBook.Domain.Storage;
using MyRecipeBook.Exception;
using MyRecipeBook.Exception.ExceptionsBase;
using System.Data.SqlTypes;

namespace MyRecipeBook.Application.UseCases.Recipe.Delete;

public class DeleteRecipeByIdUseCase : IDeleteRecipeByIdUseCase
{
    private readonly ILoggedUser _loggedUser;
    private readonly IRecipeWriteOnlyRepository _repository;
    private readonly IStorageService _storageService;

    public DeleteRecipeByIdUseCase(
        ILoggedUser loggedUser, 
        IRecipeWriteOnlyRepository repository,
        IStorageService storageService)
    {
        _loggedUser = loggedUser;
        _repository = repository;
        _storageService = storageService;

    }
    public async Task Execute(Guid recipeId)
    {
        var userId = _loggedUser.GetUserId();
        
        var deleted = await _repository.DeleteById(recipeId, userId);
        if (deleted is false)
            throw new NotFoundException(ResourceMessagesException.VALIDATION_RECIPE_NOT_FOUND);

        await _storageService.DeleteRecipeIllustration(userId, recipeId);
    }
}
