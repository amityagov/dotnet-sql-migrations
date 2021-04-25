using DotnetMigrations.Lib;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetMigrations.Command
{
	public static class DependencyInjectionExtensions
	{
		public static IMigrationsServicesBuilder AddCommands(this IMigrationsServicesBuilder builder)
		{
			builder.Services.AddSingleton<IConnectionStringsProcessorCollection, ConnectionStringsProcessorCollection>();

			builder.Services.AddSingleton<IConnectionStringsProcessor, NpgsqlConnectionStringsProcessor>();
			builder.Services.AddSingleton<IConnectionStringsProcessor, SqlServerConnectionStringsProcessor>();
			builder.Services.AddSingleton<IConnectionStringsProcessor, SqliteConnectionStringsProcessor>();

			builder.Services.AddSingleton<MigrationOptionsLoader>();
			builder.Services.AddSingleton<CommandMigrationRunner>();

			return builder;
		}
	}
}
