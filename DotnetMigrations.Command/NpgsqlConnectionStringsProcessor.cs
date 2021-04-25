using System;
using System.Collections.Generic;
using DotnetMigrations.Lib;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DotnetMigrations.Command
{
	public class NpgsqlConnectionStringsProcessor : IConnectionStringsProcessor
	{
		private readonly ILogger _logger;

		public string Type { get; } = Providers.Npgsql;

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
				var portString = environmentVariables[EnvironmentVariables.Port];

				if (int.TryParse(portString, out var port))
				{
					builder.Port = port;
				}
				else
				{
					_logger.LogError("Failed to parse port from environment variable, provided value: {Port}",
						portString);

					throw new Exception(
						$"Failed to parse port from environment variable, provided value: {portString}.");
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

				_logger.LogInformation("Use connection string: \"{ConnectionString}\"",
					sensoredPasswordBuilder.ConnectionString);
			}

			return builder.ConnectionString;
		}
	}
}
