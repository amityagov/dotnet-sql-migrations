using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DotnetMigrations.Lib.Models
{
    public class MigrationOptions
    {
        [Required]
        public string ConnectionString { get; set; }

        [Required]
        public ICollection<string> MigrationsDirectories { get; set; }

        public string ProviderType { get; set; }
    }
}
