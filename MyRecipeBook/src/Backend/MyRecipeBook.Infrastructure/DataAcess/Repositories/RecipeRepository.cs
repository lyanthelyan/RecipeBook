using Microsoft.EntityFrameworkCore;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Repositories.Recipe;

namespace MyRecipeBook.Infrastructure.DataAcess.Repositories;

internal sealed class RecipeRepository : IRecipeWriteOnlyRepository, IRecipeReadOnlyRepository
{
    private readonly MyRecipeBookDbContext _dbContext;
    public RecipeRepository(MyRecipeBookDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Add(Recipe recipe)
    {
        await _dbContext.Recipes.AddAsync(recipe);
    }

    public async Task<Recipe?> GetById(Guid recipeId, Guid userId)
    {
        return await _dbContext.Recipes
            .AsNoTracking()
            .Include(recipe => recipe.Instructions.OrderBy(instruction => instruction.Order))
            .Include(recipe => recipe.DishTypes)
            .Include(recipe => recipe.Ingredients)
            .FirstOrDefaultAsync(recipe =>
                recipe.Active &&
                recipe.Id == recipeId &&
                recipe.UserId == userId);

    }
    public async Task<bool> DeleteById(Guid recipeId, Guid userId)
    {
        var rows =  await _dbContext.Recipes
            .Where(recipe =>
                recipe.Active &&
                recipe.Id == recipeId &&
                recipe.UserId == userId)
            .ExecuteDeleteAsync();

        return rows > 0;

    }
}
