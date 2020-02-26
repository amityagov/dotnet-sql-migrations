using System;

namespace DotnetMigrations.Command
{
	public static class Help
	{
		public static string GetHelpText()
		{
			var environmentVariablesAllKeys = string.Join(Environment.NewLine, EnvironmentVariables.AllKeys);

			var helpText = $"{Environment.NewLine}" +
						   $"SQL migration file name should be formatted as: \"YYYYMMDDNN - create table Test.sql\", where NN - sequential number of migration for corresponding date." +
						   $"{Environment.NewLine}" +
						   $"{Environment.NewLine}" +
						   $"You can use these environment variables to override values:{Environment.NewLine}" +
						   $"{environmentVariablesAllKeys}{Environment.NewLine}" +
						   $"{Environment.NewLine}" +
						   $"Every specified environment value will overwrite previously specified values from configuration files or command line arguments.";

			return helpText;
		}
	}
}
