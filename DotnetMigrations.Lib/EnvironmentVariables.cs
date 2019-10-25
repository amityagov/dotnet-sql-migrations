using System.Collections.Generic;
using System.Linq;

namespace DotnetMigrations.Lib
{
	public static class EnvironmentVariables
	{
		public static string EnvironmentName = "MIGRATIONS_ENVIRONMENT";
		public static string MigrationsDirectory = "MIGRATIONS_DIRECTORY";
		public static string ConnectionStringName = "MIGRATIONS_CONNECTIONSTRING";

		public static string DatabaseName = "MIGRATIONS_DATABASE";
		public static string UserName = "MIGRATIONS_USERNAME";
		public static string Password = "MIGRATIONS_PASSWORD";
		public static string Host = "MIGRATIONS_HOST";
		public static string Port = "MIGRATIONS_PORT";

		public static string[] AllKeys = {
			ConnectionStringName,
			EnvironmentName,
			DatabaseName,
			UserName,
			Password,
			Host,
			Port,
			MigrationsDirectory
		};

		public static IDictionary<string, string> GetValues()
		{
			var variables = System.Environment.GetEnvironmentVariables();
			var target = new Dictionary<string, string>();

			foreach (string key in variables.Keys)
			{
				if (AllKeys.Contains(key))
				{
					target[key] = variables[key].ToString();
				}
			}

			return target;
		}
	}
}
