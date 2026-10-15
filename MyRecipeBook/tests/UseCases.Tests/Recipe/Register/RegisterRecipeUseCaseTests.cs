using CommonTestUtilities.Entities;
using CommonTestUtilities.Files;
using CommonTestUtilities.Identity;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Storage;
using MyRecipeBook.Application.Mappings;
using MyRecipeBook.Application.UseCases.Recipe.Register;
using MyRecipeBook.Exception;
using MyRecipeBook.Exception.ExceptionsBase;
using Shouldly;
using System.Net;

namespace UseCases.Tests.Recipe.Register;

public class RegisterRecipeUseCaseTests
{
    [Fact]
    public async Task Success()
    {
        // Arrange
        MapsterConfiguration.Configure();

        var request = RequestRecipeJsonBuilder.Build();

        var (user, _) = UserBuilder.Build();

        var useCase = CreateUseCase(user);

        var png = FileBuilder.GetPng();
        // Act
        var result = await useCase.Execute(request, png);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Title.ShouldBe(request.Title);
    }

    [Fact]
    public async Task Validate_ShouldThrowException_WhenTitleIsEmpty()
    {
        var request = RequestRecipeJsonBuilder.Build();
        request.Title = string.Empty;

        var (user, _) = UserBuilder.Build();

        var useCase = CreateUseCase(user);

        var exception = await useCase.Execute(request, recipeIllustration: null).ShouldThrowAsync<ErrorOnValidationException>();
        exception.GetStatusCode().ShouldBe(HttpStatusCode.BadRequest);
        exception.GetErrorMessages().ShouldSatisfyAllConditions(errorMessages =>
        {
            errorMessages.Count.ShouldBe(1);
            errorMessages.ShouldContain(ResourceMessagesException.VALIDATION_RECIPE_TITLE_REQUIRED);
        });
    }

    [Fact]
    public async Task Success_WithoutImage()
    {
        // Arrange
        MapsterConfiguration.Configure();

        var request = RequestRecipeJsonBuilder.Build();

        var (user, _) = UserBuilder.Build();

        var useCase = CreateUseCase(user);

        var png = FileBuilder.GetPng();
        // Act
        var result = await useCase.Execute(request, recipeIllustration: null);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Title.ShouldBe(request.Title);
        result.ImageUrl.ShouldBeEmpty();
    }

    [Fact]
    public async Task Success_WhenImageIsPng()
    {
        // Arrange
        MapsterConfiguration.Configure();

        var request = RequestRecipeJsonBuilder.Build();

        var (user, _) = UserBuilder.Build();

        var useCase = CreateUseCase(user);

        // Act
        var result = await useCase.Execute(request, recipeIllustration: FileBuilder.GetPng());

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Title.ShouldBe(request.Title);
        result.ImageUrl.ShouldBe(IStorageServiceBuilder.FakeUrl);
    }

    [Fact]
    public async Task Success_WhenImageIsJpeg()
    {
        // Arrange
        MapsterConfiguration.Configure();

        var request = RequestRecipeJsonBuilder.Build();

        var (user, _) = UserBuilder.Build();

        var useCase = CreateUseCase(user);

        // Act
        var result = await useCase.Execute(request, recipeIllustration: FileBuilder.GetJpeg());

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Title.ShouldBe(request.Title);
        result.ImageUrl.ShouldBe(IStorageServiceBuilder.FakeUrl);
    }

    [Fact]
    public async Task Error_WhenImageIsBmp()
    {
        // Arrange
        MapsterConfiguration.Configure();

        var request = RequestRecipeJsonBuilder.Build();

        var (user, _) = UserBuilder.Build();

        var useCase = CreateUseCase(user);
        // Act
        var exception = await useCase.Execute(request, recipeIllustration: FileBuilder.GetBmp()).ShouldThrowAsync<ErrorOnValidationException>();

        // Assert
        exception.GetStatusCode().ShouldBe(HttpStatusCode.BadRequest);
        exception.GetErrorMessages().ShouldSatisfyAllConditions(errorMessages =>
        {
            errorMessages.Count.ShouldBe(1);
            errorMessages.ShouldContain(ResourceMessagesException.VALIDATION_ONLY_IMAGES_ACCEPTED);
        });
    }

    [Fact]
    public async Task Error_WhenImageIsTxt()
    {
        // Arrange
        MapsterConfiguration.Configure();

        var request = RequestRecipeJsonBuilder.Build();

        var (user, _) = UserBuilder.Build();

        var useCase = CreateUseCase(user);
        // Act
        var exception = await useCase.Execute(request, recipeIllustration: FileBuilder.GetTxt()).ShouldThrowAsync<ErrorOnValidationException>();

        // Assert
        exception.GetStatusCode().ShouldBe(HttpStatusCode.BadRequest);
        exception.GetErrorMessages().ShouldSatisfyAllConditions(errorMessages =>
        {
            errorMessages.Count.ShouldBe(1);
            errorMessages.ShouldContain(ResourceMessagesException.VALIDATION_ONLY_IMAGES_ACCEPTED);
        });
    }
    private static RegisterRecipeUseCase CreateUseCase(MyRecipeBook.Domain.Entities.User user)
    {
        var recipeRepository = IRecipeWriteOnlyRepositoryBuilder.Build();
        var loggedUser = ILoggedUserBuilder.Build(user);
        var unitOfWork = IUnitOfWorkBuilder.Build();
        var storageServiceBuilder = IStorageServiceBuilder.Build();

        return new RegisterRecipeUseCase(recipeRepository, loggedUser, unitOfWork, storageServiceBuilder);
    }

}
