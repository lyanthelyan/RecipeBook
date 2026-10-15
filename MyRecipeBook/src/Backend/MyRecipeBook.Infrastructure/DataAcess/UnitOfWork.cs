using MyRecipeBook.Domain.Repositories;

namespace MyRecipeBook.Infrastructure.DataAcess;

internal class UnitOfWork : IUnitOfWork
{
    private readonly MyRecipeBookDbContext _dbContext;

    public UnitOfWork(MyRecipeBookDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Commit()
    {
        await _dbContext.SaveChangesAsync();
    }

    //Atomic Transaction, fixes sequencial functions operations
    public async Task ExecuteInTransaction(Func<Task> operation)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            await operation();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
