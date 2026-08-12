using FluentMigrator;

namespace MyRecipeBook.Infrastructure.Migrations.Versions;

[Migration(DatabaseVersions.ADD_RECIPE_CREATED_ON, "Adding CreatedOn column to Recipes table")]
public class Version0000003 : Migration
{
    public override void Up()
    {
        Alter.Table("Recipes")
            .AddColumn("CreatedOn")
            .AsDateTimeOffset()
            .NotNullable()
            .WithDefault(SystemMethods.CurrentUTCDateTime);
    }

    public override void Down()
    {
        Delete.Column("CreatedOn").FromTable("Recipes");
    }
}
