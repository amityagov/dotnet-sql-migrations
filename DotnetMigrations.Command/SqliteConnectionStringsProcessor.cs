using System;
using System.Collections.Generic;
using DotnetMigrations.Lib;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DotnetMigrations.Command
{
	public class SqliteConnectionStringsProcessor : IConnectionStringsProcessor
	{
		private readonly ILogger<SqliteConnectionStringsProcessor> _logger;

		public string Type { get; } = Providers.Sqlite;

		public SqliteConnectionStringsProcessor(ILogger<SqliteConnectionStringsProcessor> logger)
		{
			_logger = logger;
		}

		public string ProcessConnectionString(string connectionString, IDictionary<string, string> environmentVariables)
		{
			SqliteConnectionStringBuilder builder;

			if (connectionString != null)
			{
				builder = new SqliteConnectionStringBuilder(connectionString);
			}
			else
			{
				builder = new SqliteConnectionStringBuilder();
			}

			static string CreateKey(string name)
			{
				return $"{EnvironmentVariables.PrefixName}_{Providers.Sqlite}_{name}";
			}

			void TrySetValue(string key, Action<string> setter)
			{
				if (environmentVariables.TryGetValue(CreateKey(key), out string value))
				{
					try
					{
						setter(value);
					}
					catch (Exception e)
					{
						_logger.LogError(e, "Failed to set value from enviroment variable {Name}", CreateKey(key));
					}
				}
			}

			if (environmentVariables.ContainsKey(EnvironmentVariables.Password))
			{
				builder.Password = environmentVariables[EnvironmentVariables.Password];
			}

			TrySetValue(nameof(SqliteConnectionStringBuilder.DataSource), value => builder.DataSource = value);

			TrySetValue(nameof(SqliteConnectionStringBuilder.Cache),
				value => builder.Cache = Enum.Parse<SqliteCacheMode>(value));

			TrySetValue(nameof(SqliteConnectionStringBuilder.Mode),
				value => builder.Mode = Enum.Parse<SqliteOpenMode>(value));

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
