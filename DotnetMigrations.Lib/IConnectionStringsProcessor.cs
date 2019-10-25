using System.Collections.Generic;

namespace DotnetMigrations.Lib
{
	public interface IConnectionStringsProcessor
	{
		string ProcessConnectionString(string connectionString, IDictionary<string, string> environmentVariables);
	}
}