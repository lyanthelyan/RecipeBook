namespace MyRecipeBook.Domain.Entities;

public class User
{
    public bool Active { get; set; } = true;

    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

}
