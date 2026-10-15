using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Storage;

namespace MyRecipeBook.Infrastructure.Storage;

public class DisabledStorageService : IStorageService
{
    public Task DeleteRecipeIllustration(Guid userId, Guid recipeId)
    {
        return Task.CompletedTask;
    }

    public Task DeleteUserFiles(User user)
    {
        return Task.CompletedTask;
    }

    public string GetProfilePictureUrl(User user)
    {
        return string.Empty;
    }

    public string GetRecipeIllustrationUrl(Guid userId, Guid recipeId)
    {
        return string.Empty;
    }

    public Task UploadIllustration(Recipe recipe, Stream file, string contentType)
    {
        return Task.CompletedTask;
    }

    public Task UploadProfilePicture(User user, Stream file, string contentType)
    {
        return Task.CompletedTask;
    }
}
