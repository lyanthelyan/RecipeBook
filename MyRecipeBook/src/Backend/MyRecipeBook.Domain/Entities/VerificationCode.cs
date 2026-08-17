using MyRecipeBook.Domain.Enums;

namespace MyRecipeBook.Domain.Entities;

public class VerificationCode : EntityBase
{

    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;

    public string Code { get; set; } = string.Empty;
    public VerificationCodeType Type { get; set; }
    public Guid UserId { get; set; }
}
