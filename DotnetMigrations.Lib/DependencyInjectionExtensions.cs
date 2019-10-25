using Microsoft.Extensions.DependencyInjection;

namespace DotnetMigrations.Lib
{
	public static class DependencyInjectionExtensions
	{
		public static void AddMigrations(this IServiceCollection collection)
		{
			collection.AddSingleton<MigrationOptionsLoader>();
			collection.AddSingleton<MigrationRunner>();
			collection.AddSingleton<IConnectionStringsProcessor, NpgsqlConnectionStringsProcessor>();
			collection.AddSingleton<IMigrationExecutor, NpgsqlMigrationExecutor>();
		}
	}
}