using MyRecipeBook.Domain.Entities;

namespace MyRecipeBook.Domain.Security.Tokens;

public interface IAcessTokensGenerator
{
    string Generate(User user);
}
