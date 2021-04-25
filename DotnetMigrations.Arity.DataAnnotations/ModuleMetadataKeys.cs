namespace DotnetMigrations.Arity.DataAnnotations
{
    public static class ModuleMetadataKeys
    {
        public static readonly string PathKey = $"{typeof(SqlFolderAttribute).FullName}_{nameof(SqlFolderAttribute)}";
    }
}
