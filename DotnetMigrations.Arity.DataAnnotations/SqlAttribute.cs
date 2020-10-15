using System;
using Arity;

namespace DotnetMigrations.Arity.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SqlFolderAttribute : ModuleMetadataAttribute
    {
        public string Path { get; }

        public SqlFolderAttribute(string path)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public override void Apply(ModuleMetadata metadata)
        {
            metadata.Items[ModuleMetadataKeys.PathKey] = Path;
        }
    }
}
