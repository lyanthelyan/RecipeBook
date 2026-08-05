using FluentValidation;
using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Exception;

namespace MyRecipeBook.Application.UseCases.Recipe;

public class RecipeValidator : AbstractValidator<RequestRecipeJson>
{
    public RecipeValidator()
    {
        RuleFor(request => request.Title)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ResourceMessagesException.VALIDATION_RECIPE_TITLE_REQUIRED)
            .MaximumLength(250)
            .WithMessage(ResourceMessagesException.VALIDATION_RECIPE_TITLE_MAX_LENGTH);

        RuleFor(request => request.CookTime)
            .IsInEnum()
            .WithMessage(ResourceMessagesException.VALIDATION_RECIPE_COOK_TIME_INVALID);

        RuleFor(request => request.Ingredients)
            .NotNull()
            .WithMessage(ResourceMessagesException.VALIDATION_RECIPE_INGREDIENTS_REQUIRED);

        RuleForEach(request => request.Ingredients)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ResourceMessagesException.VALIDATION_RECIPE_INGREDIENT_REQUIRED)
            .MaximumLength(250)
            .WithMessage(ResourceMessagesException.VALIDATION_RECIPE_INGREDIENT_MAX_LENGTH);

        RuleFor(request => request.Instructions)
            .NotNull()
            .WithMessage(ResourceMessagesException.VALIDATION_RECIPE_INSTRUCTIONS_REQUIRED);

        RuleForEach(recipe => recipe.Instructions).ChildRules(instruction =>
        {
            instruction.RuleFor(item => item.Order)
                .GreaterThan(0)
                .WithMessage(ResourceMessagesException.VALIDATION_RECIPE_INSTRUCTION_ORDER_INVALID);

            instruction.RuleFor(item => item.Description)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(ResourceMessagesException.VALIDATION_RECIPE_INSTRUCTION_REQUIRED)
                .MaximumLength(2000)
                .WithMessage(ResourceMessagesException.VALIDATION_RECIPE_INSTRUCTION_MAX_LENGTH);
        });

        RuleFor(request => request.DishTypes)
            .NotNull()
            .WithMessage(ResourceMessagesException.VALIDATION_RECIPE_DISH_TYPES_REQUIRED);

        RuleForEach(request => request.DishTypes)
            .IsInEnum()
            .WithMessage(ResourceMessagesException.VALIDATION_RECIPE_DISH_TYPE_INVALID);
    }
}
