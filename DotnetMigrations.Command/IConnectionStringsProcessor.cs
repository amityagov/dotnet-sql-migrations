using System.Collections.Generic;

namespace DotnetMigrations.Command
{
    public interface IConnectionStringsProcessor
    {
        string ProcessConnectionString(string connectionString, IDictionary<string, string> environmentVariables);

        string Type { get; }
    }
}
