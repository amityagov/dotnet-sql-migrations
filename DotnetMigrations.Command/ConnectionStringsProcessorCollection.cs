using System;
using System.Collections.Generic;
using System.Linq;

namespace DotnetMigrations.Command
{
    public interface IConnectionStringsProcessorCollection
    {
        IConnectionStringsProcessor GetConnectionStringsProcessor(string type);
    }

    public class ConnectionStringsProcessorCollection : IConnectionStringsProcessorCollection
    {
        private readonly Dictionary<string, IConnectionStringsProcessor> _connectionStringsProcessors;

        public ConnectionStringsProcessorCollection(
            IEnumerable<IConnectionStringsProcessor> connectionStringsProcessors)
        {
            _connectionStringsProcessors =
                connectionStringsProcessors.ToDictionary(x => x.Type, StringComparer.InvariantCultureIgnoreCase);
        }

        public IConnectionStringsProcessor GetConnectionStringsProcessor(string type)
        {
            return _connectionStringsProcessors[type];
        }
    }
}
