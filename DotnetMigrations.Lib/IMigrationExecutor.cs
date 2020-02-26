using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotnetMigrations.Lib.Models;

namespace DotnetMigrations.Lib
{
	public interface IMigrationExecutor
	{
		/// <summary>
		/// <exception cref="Exception">If migrations failed</exception>
		/// </summary>
		Task ExecuteAsync(string connectionString, ICollection<MigrationInfo> files, bool dryRun,
			CancellationToken cancellationToken);

		string Type { get; }
	}
}
