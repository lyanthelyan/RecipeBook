using Mapster;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;
using MyRecipeBook.Domain.Identity;
using MyRecipeBook.Domain.Repositories;
using MyRecipeBook.Domain.Repositories.Recipe;
using MyRecipeBook.Exception.ExceptionsBase;

namespace MyRecipeBook.Application.UseCases.Recipe.Register;

public class RegisterRecipeUseCase : IRegisterRecipeUseCase
{
    private readonly IRecipeWriteOnlyRepository _recipeRepository;
    private readonly ILoggedUser _loggedUser;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterRecipeUseCase(
        IRecipeWriteOnlyRepository recipeRepository,
        ILoggedUser loggedUser, 
        IUnitOfWork unitOfWork)
    {
        _recipeRepository = recipeRepository;
        _loggedUser = loggedUser;
        _unitOfWork = unitOfWork;
    }
    public async Task<ResponseRegisteredRecipeJson> Execute(RequestRecipeJson request)
    {
        Validate(request);
        var recipe = request.Adapt<Domain.Entities.Recipe>();
        recipe.UserId = _loggedUser.GetUserId();

        await _recipeRepository.Add(recipe);
        await _unitOfWork.Commit();

        return new ResponseRegisteredRecipeJson
        {
            Id = recipe.Id,
            Title = recipe.Title
        };
    }

    private static void Validate(RequestRecipeJson request)
    {
        var result = new RecipeValidator().Validate(request);

        if (result.IsValid is false)
            throw new ErrorOnValidationException(result.Errors.Select(e => e.ErrorMessage).ToList());
    }
}
