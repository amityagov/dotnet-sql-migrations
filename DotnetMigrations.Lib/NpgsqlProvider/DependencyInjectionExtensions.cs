using Microsoft.Extensions.DependencyInjection;

namespace DotnetMigrations.Lib.NpgsqlProvider
{
	public static class DependencyInjectionExtensions
	{
		public static IMigrationsServicesBuilder AddNpgsqlMigrations(this IMigrationsServicesBuilder builder)
		{
			builder.Services.AddSingleton<IConnectionStringsProcessor, NpgsqlConnectionStringsProcessor>();
			builder.Services.AddSingleton<IMigrationExecutor, NpgsqlMigrationExecutor>();
			return builder;
		}
	}
}