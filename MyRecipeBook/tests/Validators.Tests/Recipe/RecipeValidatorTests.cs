using CommonTestUtilities.Requests;
using MyRecipeBook.Application.UseCases.Recipe;
using MyRecipeBook.Communication.Enums;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Exception;
using Shouldly;
using System.Diagnostics.CodeAnalysis;

namespace Validators.Tests.Recipe;

public class RecipeValidatorTests
{
    [Fact]
    public void Success()
    {
        var validator = new RecipeValidator();

        var request = RequestRecipeJsonBuilder.Build();

        var result = validator.Validate(request);

        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("      ")]
    [SuppressMessage("Usage", "xUnit1012:Null should only be used for nullable parameters", Justification = "Intentional")]
    public void Validate_ShouldHaveError_WhenTitleIsEmpty(string title)
    {
        var validator = new RecipeValidator();

        var request = RequestRecipeJsonBuilder.Build();
        request.Title = title;

        var result = validator.Validate(request);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldSatisfyAllConditions(errors =>
        {
            errors.Count.ShouldBe(1);
            errors.ShouldContain(error => error.ErrorMessage.Equals(ResourceMessagesException.VALIDATION_RECIPE_TITLE_REQUIRED));
        });
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenTitleExceedsMaximumLength()
    {
        var validator = new RecipeValidator();

        var request = RequestRecipeJsonBuilder.Build();
        request.Title = new string('a', 251);

        var result = validator.Validate(request);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldSatisfyAllConditions(errors =>
        {
            errors.Count.ShouldBe(1);
            errors.ShouldContain(error => error.ErrorMessage.Equals(ResourceMessagesException.VALIDATION_RECIPE_TITLE_MAX_LENGTH));
        });
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenCookTimeIsInvalid()
    {
        var validator = new RecipeValidator();

        var request = RequestRecipeJsonBuilder.Build();
        request.CookTime = (CookTime)999;

        var result = validator.Validate(request);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldSatisfyAllConditions(errors =>
        {
            errors.Count.ShouldBe(1);
            errors.ShouldContain(error => error.ErrorMessage.Equals(ResourceMessagesException.VALIDATION_RECIPE_COOK_TIME_INVALID));
        });
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenIngredientsIsNull()
    {
        var validator = new RecipeValidator();

        var request = RequestRecipeJsonBuilder.Build();
        request.Ingredients = null!;

        var result = validator.Validate(request);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldSatisfyAllConditions(errors =>
        {
            errors.Count.ShouldBe(1);
            errors.ShouldContain(error => error.ErrorMessage.Equals(ResourceMessagesException.VALIDATION_RECIPE_INGREDIENTS_REQUIRED));
        });
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("      ")]
    [SuppressMessage("Usage", "xUnit1012:Null should only be used for nullable parameters", Justification = "Intentional")]
    public void Validate_ShouldHaveError_WhenIngredientIsEmpty(string ingredient)
    {
        var validator = new RecipeValidator();

        var request = RequestRecipeJsonBuilder.Build();
        request.Ingredients = [ingredient];

        var result = validator.Validate(request);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldSatisfyAllConditions(errors =>
        {
            errors.Count.ShouldBe(1);
            errors.ShouldContain(error => error.ErrorMessage.Equals(ResourceMessagesException.VALIDATION_RECIPE_INGREDIENT_REQUIRED));
        });
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenIngredientExceedsMaximumLength()
    {
        var validator = new RecipeValidator();

        var request = RequestRecipeJsonBuilder.Build();
        request.Ingredients = [new string('a', 251)];

        var result = validator.Validate(request);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldSatisfyAllConditions(errors =>
        {
            errors.Count.ShouldBe(1);
            errors.ShouldContain(error => error.ErrorMessage.Equals(ResourceMessagesException.VALIDATION_RECIPE_INGREDIENT_MAX_LENGTH));
        });
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenInstructionsIsNull()
    {
        var validator = new RecipeValidator();

        var request = RequestRecipeJsonBuilder.Build();
        request.Instructions = null!;

        var result = validator.Validate(request);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldSatisfyAllConditions(errors =>
        {
            errors.Count.ShouldBe(1);
            errors.ShouldContain(error => error.ErrorMessage.Equals(ResourceMessagesException.VALIDATION_RECIPE_INSTRUCTIONS_REQUIRED));
        });
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenInstructionOrdersAreDuplicated()
    {
        var validator = new RecipeValidator();

        var request = RequestRecipeJsonBuilder.Build();
        request.Instructions.Add(new RequestRecipeInstructionJson
        {
            Order = request.Instructions[0].Order,
            Description = "Second instruction"
        });

        var result = validator.Validate(request);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldSatisfyAllConditions(errors =>
        {
            errors.Count.ShouldBe(1);
            errors.ShouldContain(error => error.ErrorMessage.Equals(ResourceMessagesException.VALIDATION_RECIPE_INSTRUCTION_ORDER_DUPLICATED));
        });
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ShouldHaveError_WhenInstructionOrderIsInvalid(int order)
    {
        var validator = new RecipeValidator();

        var request = RequestRecipeJsonBuilder.Build();
        request.Instructions[0].Order = order;

        var result = validator.Validate(request);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldSatisfyAllConditions(errors =>
        {
            errors.Count.ShouldBe(1);
            errors.ShouldContain(error => error.ErrorMessage.Equals(ResourceMessagesException.VALIDATION_RECIPE_INSTRUCTION_ORDER_INVALID));
        });
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("      ")]
    [SuppressMessage("Usage", "xUnit1012:Null should only be used for nullable parameters", Justification = "Intentional")]
    public void Validate_ShouldHaveError_WhenInstructionIsEmpty(string instruction)
    {
        var validator = new RecipeValidator();

        var request = RequestRecipeJsonBuilder.Build();
        request.Instructions[0].Description = instruction;

        var result = validator.Validate(request);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldSatisfyAllConditions(errors =>
        {
            errors.Count.ShouldBe(1);
            errors.ShouldContain(error => error.ErrorMessage.Equals(ResourceMessagesException.VALIDATION_RECIPE_INSTRUCTION_REQUIRED));
        });
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenInstructionExceedsMaximumLength()
    {
        var validator = new RecipeValidator();

        var request = RequestRecipeJsonBuilder.Build();
        request.Instructions[0].Description = new string('a', 2001);

        var result = validator.Validate(request);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldSatisfyAllConditions(errors =>
        {
            errors.Count.ShouldBe(1);
            errors.ShouldContain(error => error.ErrorMessage.Equals(ResourceMessagesException.VALIDATION_RECIPE_INSTRUCTION_MAX_LENGTH));
        });
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDishTypesIsNull()
    {
        var validator = new RecipeValidator();

        var request = RequestRecipeJsonBuilder.Build();
        request.DishTypes = null!;

        var result = validator.Validate(request);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldSatisfyAllConditions(errors =>
        {
            errors.Count.ShouldBe(1);
            errors.ShouldContain(error => error.ErrorMessage.Equals(ResourceMessagesException.VALIDATION_RECIPE_DISH_TYPES_REQUIRED));
        });
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDishTypesIsEmpty()
    {
        var validator = new RecipeValidator();

        var request = RequestRecipeJsonBuilder.Build();
        request.DishTypes = [];

        var result = validator.Validate(request);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldSatisfyAllConditions(errors =>
        {
            errors.Count.ShouldBe(1);
            errors.ShouldContain(error => error.ErrorMessage.Equals(ResourceMessagesException.VALIDATION_RECIPE_DISH_TYPES_REQUIRED));
        });
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDishTypeIsInvalid()
    {
        var validator = new RecipeValidator();

        var request = RequestRecipeJsonBuilder.Build();
        request.DishTypes = [(DishType)999];

        var result = validator.Validate(request);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldSatisfyAllConditions(errors =>
        {
            errors.Count.ShouldBe(1);
            errors.ShouldContain(error => error.ErrorMessage.Equals(ResourceMessagesException.VALIDATION_RECIPE_DISH_TYPE_INVALID));
        });
    }
}
