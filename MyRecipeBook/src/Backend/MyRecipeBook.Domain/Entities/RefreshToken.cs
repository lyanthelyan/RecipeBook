namespace MyRecipeBook.Domain.Entities;

public class RefreshToken : EntityBase
{
    public string Value { get; set; } = string.Empty;
    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;
    public Guid UserId { get; set; }
}
