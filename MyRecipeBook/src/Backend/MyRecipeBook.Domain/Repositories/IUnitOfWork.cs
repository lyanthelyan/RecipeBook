namespace MyRecipeBook.Domain.Repositories;

public interface IUnitOfWork
{
    Task Commit();
    Task ExecuteInTransaction(Func<Task> operation);
}
