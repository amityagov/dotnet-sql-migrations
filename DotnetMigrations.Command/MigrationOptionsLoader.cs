using System;
using System.Collections.Generic;
using System.IO;
using DotnetMigrations.Lib;
using DotnetMigrations.Lib.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DotnetMigrations.Command
{
    public class MigrationOptionsLoader
    {
        public class ConfigurationValues
        {
            public string MigrationsDirectory { get; set; }

            public string[] MigrationsDirectories { get; set; }

            public string Provider { get; set; }
        }

        private const string DefaultConfigFileName = "migrations-config.json";

        private const string DefaultConnectionStringConfigFileKey = "Default";

        private readonly ILogger _logger;

        private readonly IProviderCollection _providerCollection;
        private readonly IConnectionStringsProcessorCollection _connectionStringsProcessorCollection;

        public MigrationOptionsLoader(ILogger<MigrationOptionsLoader> logger,
            IProviderCollection providerCollection,
            IConnectionStringsProcessorCollection connectionStringsProcessorCollection)
        {
            _logger = logger;
            _providerCollection = providerCollection;
            _connectionStringsProcessorCollection = connectionStringsProcessorCollection;
        }

        public bool TryGetOptions(string configFilePath,
            string connectionString,
            string migrationsDirectory,
            string connectionStringName,
            string environmentName,
            IDictionary<string, string> environmentVariables,
            string providerType,
            out MigrationOptions options)
        {
            options = null;

            configFilePath = ProcessConfigFilePath(configFilePath);

            if (configFilePath == null)
            {
                return false;
            }

            var configuration = BuildConfiguration(configFilePath, environmentName);

            if (string.IsNullOrEmpty(connectionString))
            {
                connectionStringName ??= DefaultConnectionStringConfigFileKey;
                connectionString = configuration.GetConnectionString(connectionStringName);
            }

            var configurationValues = new ConfigurationValues();
            configuration.Bind(configurationValues);

            if (string.IsNullOrEmpty(migrationsDirectory))
            {
                if (configurationValues.MigrationsDirectories != null &&
                    configurationValues.MigrationsDirectories.Length > 0)
                {
                    migrationsDirectory = string.Join(",", configurationValues.MigrationsDirectories);
                }
            }

            if (string.IsNullOrEmpty(migrationsDirectory))
            {
                migrationsDirectory = configurationValues.MigrationsDirectory;
            }

            providerType ??= configurationValues.Provider;

            if (providerType == null)
            {
                providerType = Providers.Default;

                _logger.LogWarning("Use default provider: {ProviderType}", providerType);
            }
            else
            {
                if (!Providers.IsProviderValid(providerType))
                {
                    _logger.LogError("Unsupported provider: {ProviderType}", providerType);

                    return false;
                }
            }

            TryOverwriteSettingsByEnvironmentVariables(environmentVariables, out var overwrittenMigrationsDirectory,
                out var overwrittenConnectionString);

            migrationsDirectory = overwrittenMigrationsDirectory ?? migrationsDirectory;
            connectionString = overwrittenConnectionString ?? connectionString;

            var hasErrors = false;

            var migrationsDirectories = new List<string>();

            if (string.IsNullOrEmpty(migrationsDirectory))
            {
                _logger.LogError("Migrations directory does not specified");
                hasErrors = true;
            }
            else
            {
                foreach (var directory in migrationsDirectory.Split(new[] { "," },
                             StringSplitOptions.RemoveEmptyEntries))
                {
                    var localDirectory = directory;

                    if (!Path.IsPathRooted(localDirectory))
                    {
                        localDirectory = Path.Combine(Directory.GetCurrentDirectory(), localDirectory);
                    }

                    if (!Directory.Exists(localDirectory))
                    {
                        _logger.LogError("Migrations directory {LocalDirectory} does not exist", localDirectory);

                        hasErrors = true;
                    }
                    else
                    {
                        migrationsDirectories.Add(localDirectory);
                    }
                }
            }

            var connectionStringsProcessor =
                _connectionStringsProcessorCollection.GetConnectionStringsProcessor(providerType);

            connectionString =
                connectionStringsProcessor.ProcessConnectionString(connectionString, environmentVariables);

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("Connection string does not specified");
                hasErrors = true;
            }

            if (hasErrors)
            {
                return false;
            }

            options = new MigrationOptions
            {
                ConnectionString = connectionString,
                MigrationsDirectories = migrationsDirectories,
                ProviderType = providerType
            };

            return true;
        }

        private string ProcessConfigFilePath(string configFilePath)
        {
            if (!string.IsNullOrEmpty(configFilePath))
            {
                if (!File.Exists(configFilePath))
                {
                    _logger.LogError("Config file name specified but not found at \"{ConfigFilePath}\"",
                        configFilePath);
                    return null;
                }
            }
            else
            {
                var defaultConfigFilePath = Path.Combine(Directory.GetCurrentDirectory(), DefaultConfigFileName);

                if (File.Exists(defaultConfigFilePath))
                {
                    _logger.LogInformation("Default config file found at \"{DefaultConfigFilePath}\"",
                        defaultConfigFilePath);

                    configFilePath = defaultConfigFilePath;
                }
            }

            if (configFilePath != null)
            {
                var isPathFullyQualified = Path.IsPathFullyQualified(configFilePath);

                if (isPathFullyQualified == false)
                {
                    configFilePath = Path.Combine(Directory.GetCurrentDirectory(), configFilePath);
                }
            }

            return configFilePath;
        }

        private IConfiguration BuildConfiguration(string configFilePath, string environmentName)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();

            if (File.Exists(configFilePath))
            {
                builder = builder.AddJsonFile(configFilePath, false);
            }

            var environmentNameFormEnvironment =
                Environment.GetEnvironmentVariable(EnvironmentVariables.EnvironmentName);

            environmentName = environmentNameFormEnvironment ?? environmentName;

            if (environmentName != null)
            {
                var basePath = Path.GetDirectoryName(configFilePath);
                var fileName = Path.GetFileName(configFilePath);

                var environmentConfigFileName = Path.ChangeExtension(fileName,
                    $"{environmentName.ToLower()}{Path.GetExtension(fileName)}");

                if (File.Exists(Path.Combine(basePath, environmentConfigFileName)))
                {
                    _logger.LogInformation("Use environment specific config file: {ConfigFileName}",
                        Path.Combine(basePath, environmentConfigFileName));

                    builder = builder.AddJsonFile(Path.Combine(basePath, environmentConfigFileName), false);
                }
            }

            var configuration = builder.Build();

            return configuration;
        }

        private static bool TryOverwriteValueByEnvironmentVariable(IDictionary<string, string> environmentValues,
            string name, out string value)
        {
            value = null;

            if (environmentValues.ContainsKey(name))
            {
                value = environmentValues[name];
                return true;
            }

            return false;
        }

        private static void TryOverwriteSettingsByEnvironmentVariables(IDictionary<string, string> environmentValues,
            out string migrationsDirectory, out string connectionString)
        {
            if (!TryOverwriteValueByEnvironmentVariable(environmentValues, EnvironmentVariables.MigrationsDirectories,
                    out migrationsDirectory) || string.IsNullOrEmpty(migrationsDirectory))
            {
                TryOverwriteValueByEnvironmentVariable(environmentValues, EnvironmentVariables.MigrationsDirectory,
                    out migrationsDirectory);
            }

            TryOverwriteValueByEnvironmentVariable(environmentValues, EnvironmentVariables.ConnectionStringName,
                out connectionString);
        }
    }
}
