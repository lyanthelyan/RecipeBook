using MyRecipeBook.Communication.Requests;

namespace MyRecipeBook.Application.UseCases.Recipe.Update;

public interface IUpdateRecipeByIdUseCase
{
    Task Execute(Guid recipeId, RequestRecipeJson request);
}
