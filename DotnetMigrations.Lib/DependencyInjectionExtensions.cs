using Microsoft.Extensions.DependencyInjection;

namespace DotnetMigrations.Lib
{
	public static class DependencyInjectionExtensions
	{
		public static IMigrationsServicesBuilder AddMigrations(this IServiceCollection collection)
		{
			collection.AddSingleton<MigrationOptionsLoader>();
			collection.AddSingleton<MigrationRunner>();
			collection.AddSingleton<IProviderCollection, ProviderCollection>();

			return new MigrationsServicesBuilder(collection);
		}
	}
}