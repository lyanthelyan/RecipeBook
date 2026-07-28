using MyRecipeBook.Domain.Entities;

namespace MyRecipeBook.Domain.Security.Tokens;

public interface IAccessTokensGenerator
{
    string Generate(User user);
}
