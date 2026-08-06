using FluentMigrator;
using System.Data;

namespace MyRecipeBook.Infrastructure.Migrations.Versions;

[Migration(DatabaseVersions.TABLE_RECIPES, "Creating Recipes table")]
public class Version0000002 : Migration
{
    public override void Up()
    {
        Create.Table("Recipes")
            .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("Active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("Title").AsString(250).NotNullable()
            .WithColumn("CookTime").AsInt32().NotNullable()
            .WithColumn("UserId").AsGuid().NotNullable().ForeignKey("Users", "Id");

        Create.Table("RecipeIngredients")
            .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("Active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("Item").AsString(250).NotNullable()
            .WithColumn("RecipeId").AsGuid().NotNullable().ForeignKey("Recipes", "Id").OnDelete(Rule.Cascade);

        Create.Table("RecipeInstructions")
            .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("Active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("Order").AsInt32().NotNullable()
            .WithColumn("Description").AsString(2000).NotNullable()
            .WithColumn("RecipeId").AsGuid().NotNullable().ForeignKey("Recipes", "Id").OnDelete(Rule.Cascade);

        Create.Table("RecipeDishTypes")
            .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("Active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("Type").AsInt32().NotNullable()
            .WithColumn("RecipeId").AsGuid().NotNullable().ForeignKey("Recipes", "Id").OnDelete(Rule.Cascade);
    }

    public override void Down()
    {
        Delete.Table("RecipeDishTypes");
        Delete.Table("RecipeInstructions");
        Delete.Table("RecipeIngredients");
        Delete.Table("Recipes");
    }
}
