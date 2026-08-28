using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Storage;

namespace MyRecipeBook.Infrastructure.Storage;

internal sealed class AzureStorageService : IStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private const string ProfilePictureFileName = "profile-picture";
    private const uint ProfilePictureExpirationMinutes = 60;
    private const uint RecipeIllustrationExpirationMinutes = 60;
    public AzureStorageService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }
    public async Task UploadProfilePicture(User user, Stream file, string contentType)
    {
        await Upload(user.Id, file, ProfilePictureFileName, contentType);
    }
    
    public async Task UploadIllustration(Recipe recipe, Stream file, string contentType)
    {
        await Upload(recipe.UserId, file, recipe.Id.ToString(), contentType);
    }

    public string GetProfilePictureUrl(User user)
    {
        return GenerateReadUrl(user.Id, ProfilePictureFileName, ProfilePictureExpirationMinutes);
    }

    public string GetRecipeIllustrationUrl(Guid userId, Guid recipeId)
    {
        return GenerateReadUrl(userId, recipeId.ToString(), RecipeIllustrationExpirationMinutes);
    }

    private async Task Upload(Guid userId, Stream file, string blobName, string contentType)
    {
        var containerClient = _blobServiceClient
            .GetBlobContainerClient(userId.ToString());
        
        await containerClient
            .CreateIfNotExistsAsync();

        var blobClient = containerClient
            .GetBlobClient(blobName);
        
        await blobClient.UploadAsync(file, new BlobHttpHeaders
        { 
                ContentType = contentType   
        });
    }

    private string GenerateReadUrl(Guid userId, string blobName, uint expirationInMinutes)
    {
        var blob = _blobServiceClient
          .GetBlobContainerClient(userId.ToString())
          .GetBlobClient(blobName);
       
        return blob
            .GenerateSasUri(
                BlobSasPermissions.Read, 
                DateTime.UtcNow.AddMinutes(expirationInMinutes))
            .ToString();
    }

    public async Task DeleteUserFiles(User user)
    {
        var containterClient = _blobServiceClient.GetBlobContainerClient(user.Id.ToString());

        await containterClient.DeleteIfExistsAsync();
    }

    public async Task DeleteRecipeIllustration(Guid userId, Guid recipeId)
    {
        var blob = _blobServiceClient
            .GetBlobContainerClient(userId.ToString())
            .GetBlobClient(recipeId.ToString());

        await blob.DeleteIfExistsAsync();
    }
}
