using FluentMigrator;

namespace MyRecipeBook.Infrastructure.Migrations.Versions;

[Migration(DatabaseVersions.VALIDATION_WITH_TOKEN_VERSION, "Adding TokenVersion column to Users table")]
public class Version0000007 : ForwardOnlyMigration
{
    public override void Up()
    {
        Alter.Table("Users")
            .AddColumn("TokenVersion")
            .AsInt32()
            .NotNullable()
            .WithDefaultValue(1);
    }

}
