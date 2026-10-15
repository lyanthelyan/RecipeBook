using MyRecipeBook.Application.Extensions;
using MyRecipeBook.Domain.Identity;
using MyRecipeBook.Domain.Repositories;
using MyRecipeBook.Domain.Repositories.Recipe;
using MyRecipeBook.Domain.Storage;
using MyRecipeBook.Exception;
using MyRecipeBook.Exception.ExceptionsBase;

namespace MyRecipeBook.Application.UseCases.Recipe.ChangeIllustration;

public class ChangeIllustrationUseCase : IChangeIllustrationUseCase
{
    private readonly ILoggedUser _loggedUser;
    private readonly IRecipeUpdateOnlyRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStorageService _storageService;
    public ChangeIllustrationUseCase(
        ILoggedUser loggedUser,
        IRecipeUpdateOnlyRepository repository,
        IUnitOfWork unitOfWork,
        IStorageService storageService)
    {
        _loggedUser = loggedUser;
        _repository = repository;
        _unitOfWork = unitOfWork;
        _storageService = storageService;
    }

    public async Task Execute(Guid recipeId, Stream recipeIllustration)
    {
        var contentType = recipeIllustration.DetectImageContentType();
        if (contentType is null)
            throw new ErrorOnValidationException([ResourceMessagesException.VALIDATION_ONLY_IMAGES_ACCEPTED]);

        var userId = _loggedUser.GetUserId();

        var recipe = await _repository.GetById(recipeId, userId);
        if (recipe is null)
            throw new NotFoundException(ResourceMessagesException.VALIDATION_RECIPE_NOT_FOUND);

        await _storageService.UploadIllustration(recipe, recipeIllustration, contentType);

        recipe.HasImage = true;

        await _unitOfWork.Commit();

    }
}
