using System;
using System.Collections.Generic;
using DotnetMigrations.Lib;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace DotnetMigrations.Command
{
    public class SqlServerConnectionStringsProcessor : IConnectionStringsProcessor
    {
        private readonly ILogger _logger;

        public string Type { get; } = Providers.SqlServer;

        public SqlServerConnectionStringsProcessor(ILogger<SqlServerConnectionStringsProcessor> logger)
        {
            _logger = logger;
        }

        public string ProcessConnectionString(string connectionString, IDictionary<string, string> environmentVariables)
        {
            SqlConnectionStringBuilder builder;

            if (connectionString != null)
            {
                builder = new SqlConnectionStringBuilder(connectionString);
            }
            else
            {
                builder = new SqlConnectionStringBuilder();
            }

            var dataSource = builder.DataSource;

            var setDataSource = false;

            if (environmentVariables.ContainsKey(EnvironmentVariables.Host))
            {
                dataSource = environmentVariables[EnvironmentVariables.Host];
                setDataSource = true;
            }

            if (environmentVariables.ContainsKey(EnvironmentVariables.Port))
            {
                var portString = environmentVariables[EnvironmentVariables.Port];
                if (int.TryParse(portString, out var port))
                {
                    dataSource = $"{dataSource},{port}";
                }
                else
                {
                    _logger.LogError("Failed to parse port from environment variable, provided value: {Port}",
                        portString);

                    throw new Exception(
                        $"Failed to parse port from environment variable, provided value: {portString}.");
                }
            }

            if (setDataSource)
            {
                builder.DataSource = dataSource;
            }

            if (environmentVariables.ContainsKey(EnvironmentVariables.DatabaseName))
            {
                builder.InitialCatalog = environmentVariables[EnvironmentVariables.DatabaseName];
            }

            if (environmentVariables.ContainsKey(EnvironmentVariables.UserName))
            {
                builder.UserID = environmentVariables[EnvironmentVariables.UserName];
            }

            if (environmentVariables.ContainsKey(EnvironmentVariables.Password))
            {
                builder.Password = environmentVariables[EnvironmentVariables.Password];
            }

            if (builder.ConnectionString.Length > 0)
            {
                var sensoredPasswordBuilder = new SqlConnectionStringBuilder(builder.ConnectionString)
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
