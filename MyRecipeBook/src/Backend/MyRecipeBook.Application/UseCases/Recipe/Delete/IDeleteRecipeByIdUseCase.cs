namespace MyRecipeBook.Application.UseCases.Recipe.Delete;

public interface IDeleteRecipeByIdUseCase
{
   Task Execute(Guid recipeId);

}
