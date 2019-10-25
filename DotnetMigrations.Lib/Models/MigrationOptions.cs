using System.ComponentModel.DataAnnotations;

namespace DotnetMigrations.Lib.Models
{
	public class MigrationOptions
	{
		[Required]
		public string ConnectionString { get; set; }

		[Required]
		public string MigrationsDirectory { get; set; }
	}
}
