using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.FileProviders;

namespace DotnetMigrations.Lib
{
	public class MigrationRunnerOptions
	{
		/// <summary>
		/// <see cref="Providers.Npgsql"/>
		/// <see cref="Providers.SqlServer"/>
		/// </summary>
		[Required]
		public string ProviderType { get; set; } = Providers.Default;

		[Required]
		public string ConnectionString { get; set; }

		public bool DryRun { get; set; }

		[Required]
		public ICollection<IFileProvider> FileProviders { get; set; }

		[Required]
		public string Pattern { get; set; } = "*.sql";
	}
}
