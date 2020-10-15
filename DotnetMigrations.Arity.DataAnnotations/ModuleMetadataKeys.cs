namespace DotnetMigrations.Arity.DataAnnotations
{
    public class ModuleMetadataKeys
    {
        public static string PathKey = $"{typeof(SqlFolderAttribute).FullName}_{nameof(SqlFolderAttribute)}";
    }
}
