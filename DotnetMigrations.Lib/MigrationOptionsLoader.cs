using System.Collections.Generic;
using System.IO;
using DotnetMigrations.Lib.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DotnetMigrations.Lib
{
	public class MigrationOptionsLoader
	{
		private const string DefaultConfigFileName = "migrations-config.json";

		private const string DefaultConnectionStringConfigFileKey = "Default";

		private const string MigrationsDirectoryConfigFileKey = "MigrationsDirectory";

		private readonly ILogger _logger;
		private readonly IConnectionStringsProcessor _connectionStringsProcessor;

		public MigrationOptionsLoader(ILogger<MigrationOptionsLoader> logger,
			IConnectionStringsProcessor connectionStringsProcessor)
		{
			_logger = logger;
			_connectionStringsProcessor = connectionStringsProcessor;
		}

		public bool TryGetOptions(string configFilePath,
			string connectionString,
			string migrationsDirectory,
			string connectionStringName,
			string environmentName,
			IDictionary<string, string> environmentVariables,
			out MigrationOptions options)
		{
			options = null;

			var currentDirectory = Directory.GetCurrentDirectory();

			if (!string.IsNullOrEmpty(configFilePath))
			{
				if (!File.Exists(configFilePath))
				{
					_logger.LogError($"Config file name specified but not found at \"{configFilePath}\".");
					return false;
				}
			}
			else
			{
				var defaultConfigFilePath = Path.Combine(currentDirectory, DefaultConfigFileName);

				if (File.Exists(defaultConfigFilePath))
				{
					_logger.LogInformation($"Default config file found at \"{defaultConfigFilePath}\"");

					configFilePath = defaultConfigFilePath;
				}
			}

			if (configFilePath != null)
			{
				var isPathFullyQualified = Path.IsPathFullyQualified(configFilePath);

				if (isPathFullyQualified == false)
				{
					configFilePath = Path.Combine(currentDirectory, configFilePath);
				}
			}

			IConfigurationBuilder builder = new ConfigurationBuilder();

			if (File.Exists(configFilePath))
			{
				builder = builder.AddJsonFile(configFilePath, false);
			}

			var environmentNameFormEnvironment = System.Environment.GetEnvironmentVariable(EnvironmentVariables.EnvironmentName);

			environmentName = environmentNameFormEnvironment ?? environmentName;

			if (environmentName != null)
			{
				var basePath = Path.GetDirectoryName(configFilePath);
				var fileName = Path.GetFileName(configFilePath);

				var environmentConfigFileName = Path.ChangeExtension(fileName,
					$"{environmentName.ToLower()}{Path.GetExtension(fileName)}");

				if (File.Exists(Path.Combine(basePath, environmentConfigFileName)))
				{
					_logger.LogInformation(
						$"Use environment specific config file: {Path.Combine(basePath, environmentConfigFileName)}.");
					builder = builder.AddJsonFile(Path.Combine(basePath, environmentConfigFileName), false);
				}
			}

			var configuration = builder.Build();

			if (string.IsNullOrEmpty(connectionString))
			{
				connectionStringName = connectionStringName ?? DefaultConnectionStringConfigFileKey;
				connectionString = configuration.GetConnectionString(connectionStringName);
			}

			if (string.IsNullOrEmpty(migrationsDirectory))
			{
				migrationsDirectory = configuration[MigrationsDirectoryConfigFileKey];
			}

			TryOverwriteSettingsByEnvironmentVariables(environmentVariables, out var overwrittenMigrationsDirectory,
				out var overwrittenConnectionString);

			migrationsDirectory = overwrittenMigrationsDirectory ?? migrationsDirectory;
			connectionString = overwrittenConnectionString ?? connectionString;

			var hasErrors = false;

			if (string.IsNullOrEmpty(migrationsDirectory))
			{
				_logger.LogError("Migrations directory does not specified.");
				hasErrors = true;
			}
			else
			{
				if (!Path.IsPathRooted(migrationsDirectory))
				{
					migrationsDirectory = Path.Combine(currentDirectory, migrationsDirectory);
				}

				if (!Directory.Exists(migrationsDirectory))
				{
					_logger.LogError("Migrations directory does not exist.");
					hasErrors = true;
				}
			}

			connectionString = _connectionStringsProcessor.ProcessConnectionString(connectionString, environmentVariables);

			if (string.IsNullOrEmpty(connectionString))
			{
				_logger.LogError("Connection string does not specified.");
				hasErrors = true;
			}

			if (hasErrors)
			{
				return false;
			}

			options = new MigrationOptions
			{
				ConnectionString = connectionString,
				MigrationsDirectory = migrationsDirectory
			};

			return true;
		}

		private void TryOverwriteSettingsByEnvironmentVariables(IDictionary<string, string> environmentValues,
			out string migrationsDirectory, out string connectionString)
		{
			migrationsDirectory = null;
			connectionString = null;

			if (environmentValues.ContainsKey(EnvironmentVariables.MigrationsDirectory))
			{
				migrationsDirectory = environmentValues[EnvironmentVariables.MigrationsDirectory];
			}

			if (environmentValues.ContainsKey(EnvironmentVariables.ConnectionStringName))
			{
				connectionString = environmentValues[EnvironmentVariables.ConnectionStringName];
			}
		}
	}
}
