using System;
using System.Collections.Generic;
using DotnetMigrations.Lib.Models;

namespace DotnetMigrations.Lib
{
	public interface IMigrationExecutor
	{
		/// <summary>
		/// <exception cref="Exception">If migrations failed</exception>
		/// </summary>
		void Execute(string connectionString, IList<MigrationInfo> files, bool dryRun);

		string Type { get; }
	}
}
