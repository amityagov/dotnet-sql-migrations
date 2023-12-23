using System;
using DotnetMigrations.Lib.NpgsqlProvider;
using DotnetMigrations.Lib.SqliteProvider;
using DotnetMigrations.Lib.SqlServerProvider;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotnetMigrations.Lib
{
    public static class DependencyInjectionExtensions
    {
        public static IMigrationsServicesBuilder AddMigrations(this IServiceCollection collection,
            Action<IServiceProvider, MigrationRunnerOptions> action = null)
        {
            var builder = new MigrationsServicesBuilder(collection);

            builder.Services.AddSingleton<IProviderCollection, ProviderCollection>();
            builder.Services.AddSingleton<IMigrationExecutor, SqlServerMigrationExecutor>();
            builder.Services.AddSingleton<IMigrationExecutor, NpgsqlMigrationExecutor>();
            builder.Services.AddSingleton<IMigrationExecutor, SqliteMigrationExecutor>();

            builder.Services.AddSingleton<IMigrationRunner, MigrationRunner>();
            builder.Services.AddSingleton<IMigrationFilesLoader, MigrationFilesLoader>();

            if (action != null)
            {
                builder.Services.AddTransient<IConfigureOptions<MigrationRunnerOptions>>(provider =>
                    new ConfigureMigrationRunnerOptions(provider, action));
            }

            return builder;
        }
    }
}
