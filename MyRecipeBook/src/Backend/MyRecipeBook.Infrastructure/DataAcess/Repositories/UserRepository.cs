namespace MyRecipeBook.Infrastructure.DataAcess.Repositories;

internal sealed class UserRepository
{
    private readonly MyRecipeBookDbContext _dbContext;
    public UserRepository(MyRecipeBookDbContext dbContext)
    {
        dbContext = _dbContext;
    }
}
