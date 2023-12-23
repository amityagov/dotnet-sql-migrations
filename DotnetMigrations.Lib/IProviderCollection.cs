using System;
using System.Collections.Generic;
using System.Linq;

namespace DotnetMigrations.Lib
{
    public interface IProviderCollection
    {
        IMigrationExecutor GetMigrationExecutor(string type);
    }

    public class ProviderCollection : IProviderCollection
    {
        private readonly Dictionary<string, IMigrationExecutor> _migrationExecutors;

        public ProviderCollection(IEnumerable<IMigrationExecutor> migrationExecutors)
        {
            _migrationExecutors =
                migrationExecutors.ToDictionary(x => x.Type, StringComparer.InvariantCultureIgnoreCase);
        }

        public IMigrationExecutor GetMigrationExecutor(string type)
        {
            return _migrationExecutors[type];
        }
    }
}
