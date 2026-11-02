namespace MyRecipeBook.Infrastructure.Migrations;

internal abstract class DatabaseVersions
{
    internal const int TABLE_USERS = 1;
    internal const int TABLE_RECIPES = 2;
    internal const int ADD_RECIPE_CREATED_ON = 3;
    internal const int TABLE_VERIFICATION_CODES = 4;
    internal const int HAS_IMAGE_COLUMNS = 5;
    internal const int INCREASE_VERIFICATION_CODE_LENGTH = 6;
    internal const int VALIDATION_WITH_TOKEN_VERSION = 7;
    internal const int TABLE_REFRESH_TOKENS = 8;

}
