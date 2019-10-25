using System;
using System.Collections.Generic;
using System.Linq;

namespace DotnetMigrations.Lib
{
	public interface IProviderCollection
	{
		IConnectionStringsProcessor GetConnectionStringsProcessor(string type);

		IMigrationExecutor GetMigrationExecutor(string type);
	}

	public class ProviderCollection : IProviderCollection
	{
		private readonly Dictionary<string, IConnectionStringsProcessor> _connectionStringsProcessors;
		private readonly Dictionary<string, IMigrationExecutor> _migrationExecutors;

		public ProviderCollection(IEnumerable<IConnectionStringsProcessor> connectionStringsProcessors,
			IEnumerable<IMigrationExecutor> migrationExecutors)
		{
			_connectionStringsProcessors = connectionStringsProcessors.ToDictionary(x => x.Type, StringComparer.InvariantCultureIgnoreCase);

			_migrationExecutors = migrationExecutors.ToDictionary(x => x.Type, StringComparer.InvariantCultureIgnoreCase);
		}

		public IConnectionStringsProcessor GetConnectionStringsProcessor(string type)
		{
			return _connectionStringsProcessors[type];
		}

		public IMigrationExecutor GetMigrationExecutor(string type)
		{
			return _migrationExecutors[type];
		}
	}
}
