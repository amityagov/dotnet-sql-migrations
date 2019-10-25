using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DotnetMigrations.Lib.NpgsqlProvider
{
	public class NpgsqlConnectionStringsProcessor : IConnectionStringsProcessor
	{
		private readonly ILogger _logger;

		public string Type { get; } = "Npgsql";

		public NpgsqlConnectionStringsProcessor(ILogger<NpgsqlConnectionStringsProcessor> logger)
		{
			_logger = logger;
		}

		public string ProcessConnectionString(string connectionString, IDictionary<string, string> environmentVariables)
		{
			NpgsqlConnectionStringBuilder builder;

			if (connectionString != null)
			{
				builder = new NpgsqlConnectionStringBuilder(connectionString);
			}
			else
			{
				builder = new NpgsqlConnectionStringBuilder();
			}

			if (environmentVariables.ContainsKey(EnvironmentVariables.Host))
			{
				builder.Host = environmentVariables[EnvironmentVariables.Host];
			}

			if (environmentVariables.ContainsKey(EnvironmentVariables.Port))
			{
				if (int.TryParse(environmentVariables[EnvironmentVariables.Port], out var port))
				{
					builder.Port = port;
				}
				else
				{
					var errorMessage = $"Failed to parse port from environment variable, provided value: {environmentVariables[EnvironmentVariables.Port]}.";

					_logger.LogError(errorMessage);

					throw new Exception(errorMessage);
				}
			}

			if (environmentVariables.ContainsKey(EnvironmentVariables.DatabaseName))
			{
				builder.Database = environmentVariables[EnvironmentVariables.DatabaseName];
			}

			if (environmentVariables.ContainsKey(EnvironmentVariables.UserName))
			{
				builder.Username = environmentVariables[EnvironmentVariables.UserName];
			}

			if (environmentVariables.ContainsKey(EnvironmentVariables.Password))
			{
				builder.Password = environmentVariables[EnvironmentVariables.Password];
			}

			if (builder.ConnectionString.Length > 0)
			{
				var sensoredPasswordBuilder = new NpgsqlConnectionStringBuilder(builder.ConnectionString)
				{
					Password = "--HIDDEN--"
				};

				_logger.LogInformation($"Use connection string: \"{sensoredPasswordBuilder.ConnectionString}\".");
			}

			return builder.ConnectionString;
		}
	}
}
