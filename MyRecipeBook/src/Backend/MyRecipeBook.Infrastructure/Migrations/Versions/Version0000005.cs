using FluentMigrator;
namespace MyRecipeBook.Infrastructure.Migrations.Versions;

[Migration(DatabaseVersions.HAS_IMAGE_COLUMNS, "Adding HasImage column to Users and Recipes Tables")]
public class Version0000005 : ForwardOnlyMigration
{
    public override void Up()
    {
        Alter.Table("Users")
            .AddColumn("HasImage")
                .AsBoolean()
                .NotNullable()
                .SetExistingRowsTo(false);
        
        Alter.Table("Recipes")
            .AddColumn("HasImage")
                .AsBoolean()
                .NotNullable()
                .SetExistingRowsTo(false);

    }
}
