using Microsoft.Extensions.DependencyInjection;

namespace DotnetMigrations.Lib.SqlServerProvider
{
	public static class DependencyInjectionExtensions
	{
		public static IMigrationsServicesBuilder AddSqlServerMigrations(this IMigrationsServicesBuilder builder)
		{
			builder.Services.AddSingleton<IConnectionStringsProcessor, SqlServerConnectionStringsProcessor>();
			builder.Services.AddSingleton<IMigrationExecutor, SqlServerMigrationExecutor>();

			return builder;
		}
	}
}