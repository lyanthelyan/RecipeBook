using MyRecipeBook.Domain.Enums;

namespace MyRecipeBook.Domain.Dtos;

public record GeneratedRecipeDto
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Difficulty { get; init; } = string.Empty;
    public string Servings { get; init; } = string.Empty;
    public CookTime CookTime { get; init; }
    public IList<GeneratedIngredientDto> Ingredients { get; init; } = [];
    public IList<GeneratedInstructionsDto> Instructions { get; init; } = [];
    public byte[]? Image { get; init; }
}
public record GeneratedIngredientDto
{
    public string Name { get; init; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string Quantity { get; init; } = string.Empty;
}
public record GeneratedInstructionsDto
{
    public int Order { get; init; }
    public string Description { get; init; } = string.Empty;
}
