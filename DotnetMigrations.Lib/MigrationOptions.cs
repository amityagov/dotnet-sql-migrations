using System.ComponentModel.DataAnnotations;

namespace DotnetMigrations.Lib
{
	public class MigrationOptions
	{
		[Required]
		public string ConnectionString { get; set; }

		[Required]
		public string MigrationsDirectory { get; set; }
	}
}
