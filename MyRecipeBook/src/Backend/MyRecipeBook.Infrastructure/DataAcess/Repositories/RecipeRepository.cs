using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MyRecipeBook.Domain.Dtos;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Extensions;
using MyRecipeBook.Domain.Repositories.Recipe;

namespace MyRecipeBook.Infrastructure.DataAcess.Repositories;

internal sealed class RecipeRepository : IRecipeWriteOnlyRepository, IRecipeReadOnlyRepository, IRecipeUpdateOnlyRepository
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

    async Task<Recipe?> IRecipeReadOnlyRepository.GetById(Guid recipeId, Guid userId)
    {
        return await GetFullRecipe()
            .AsNoTracking()
            .FirstOrDefaultAsync(recipe =>
                recipe.Active &&
                recipe.Id == recipeId &&
                recipe.UserId == userId);

    }

    async Task<Recipe?> IRecipeUpdateOnlyRepository.GetById(Guid recipeId, Guid userId)
    {
        return await GetFullRecipe()
            .FirstOrDefaultAsync(recipe =>
                recipe.Active &&
                recipe.Id == recipeId &&
                recipe.UserId == userId);

    }

    private IIncludableQueryable<Recipe, ICollection<RecipeIngredient>> GetFullRecipe()
    {
        return _dbContext.Recipes
            .Include(recipe => recipe.Instructions.OrderBy(instruction => instruction.Order))
            .Include(recipe => recipe.DishTypes)
            .Include(recipe => recipe.Ingredients);
    }
    public async Task<bool> DeleteById(Guid recipeId, Guid userId)
    {
        var rows = await _dbContext.Recipes
            .Where(recipe =>
                recipe.Active &&
                recipe.Id == recipeId &&
                recipe.UserId == userId)
            .ExecuteDeleteAsync();

        return rows > 0;

    }

    public async Task<IList<RecipeSummaryDto>> GetRecentRecipes(Guid userId)
    {
        return await _dbContext.Recipes
            .AsNoTracking()
            .Where(recipe => recipe.Active && recipe.UserId == userId)
            .OrderByDescending(recipe => recipe.CreatedOn)
            .Take(6)
            .Select(recipe => new RecipeSummaryDto(recipe.Id, recipe.Title, recipe.HasImage))
            .ToListAsync();
    }

    public async Task<IList<RecipeSummaryDto>> FilterRecipes(Guid userId, RecipeFilterDto filter)
    {
        var query = _dbContext.Recipes
            .AsNoTracking()
            .Where(recipe => recipe.Active && recipe.UserId == userId);

        if (filter.CookTime is not null)
            query = query.Where(recipe => recipe.CookTime == filter.CookTime);

        if (filter.SearchTerm.IsNotEmpty())
            query = query.Where(recipe => recipe.Title.Contains(filter.SearchTerm) || 
            recipe.Ingredients.Any(i => i.Item.Contains(filter.SearchTerm)));

        if (filter.DishTypes.Any())
        {
            query = query.Where(recipe => 
            recipe.DishTypes.Any(dt => filter.DishTypes.Contains(dt.Type)));
        }

        return await query.Select(recipe => new RecipeSummaryDto(recipe.Id, recipe.Title, recipe.HasImage)).ToListAsync();
    }
}
 