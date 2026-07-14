using CommonTestUtilities.Rquests;
using MyRecipeBook.Application.UseCases.User.Register;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Exception;
using Shouldly;
using System.Diagnostics.CodeAnalysis;

namespace Validators.Tests.User.Register;

public class RegisterUserAccountValidatorTests
{
    [Fact]
    public void Success()
    {
        // Arrange
        var request = RequestRegisterUserAccountJsonBuilder.Build();

        var validator = new RegisterUserAccountValidator();

        // Act
        var result = validator.Validate(request);

        // Assert
        
        //Assert.True(result.IsValid);
        result.IsValid.ShouldBeTrue();      
    }
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("           ")]
    [SuppressMessage("Usage", "xUnit1012:Null should only be used for nullable parameters", Justification = "Intetional because its an unit test")]
    public void Validate_ShouldHaveError_WhenNameIsEmpty(string name)
    {
        // Arrange
        var request = RequestRegisterUserAccountJsonBuilder.Build();
        request.Name = name;
        
        var validator = new RegisterUserAccountValidator();

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldSatisfyAllConditions(errors => 
        {
            errors.Count.ShouldBe(1);
            errors.ShouldContain(error => error.ErrorMessage.Equals(ResourceMessagesException.VALIDATION_NAME_REQUIRED));
        });
    }
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("           ")]
    [SuppressMessage("Usage", "xUnit1012:Null should only be used for nullable parameters", Justification = "Intetional because its an unit test")]
    public void Validate_ShouldHaveError_WhenEmailIsEmpty(string email)
    {
        // Arrange
        var request = RequestRegisterUserAccountJsonBuilder.Build();
        request.Email = email;

        var validator = new RegisterUserAccountValidator();

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldSatisfyAllConditions(errors =>
        {
            errors.Count.ShouldBe(1);
            errors.ShouldContain(error => error.ErrorMessage.Equals(ResourceMessagesException.VALIDATION_EMAIL_REQUIRED));
        });
    }
    [Fact]
    public void Validate_ShouldHaveError_WhenPasswordIsEmpty()
    {
        // Arrange
        var request = RequestRegisterUserAccountJsonBuilder.Build();
        request.Password = string.Empty;

        var validator = new RegisterUserAccountValidator();

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldSatisfyAllConditions(errors =>
        {
            errors.Count.ShouldBe(1);
            errors.ShouldContain(error => error.ErrorMessage.Equals(ResourceMessagesException.VALIDATION_PASSWORD_REQUIRED));
        });


    }
    [Fact]
    public void Validate_ShouldHaveError_WhenEmailIsInvalid()
    {
        // Arrange
        var request = RequestRegisterUserAccountJsonBuilder.Build();
        request.Email = "test.com";

        var validator = new RegisterUserAccountValidator();

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldSatisfyAllConditions(errors =>
        {
            errors.Count.ShouldBe(1);
            errors.ShouldContain(error => error.ErrorMessage.Equals(ResourceMessagesException.VALIDATION_EMAIL_INVALID));
        });


    }

}
