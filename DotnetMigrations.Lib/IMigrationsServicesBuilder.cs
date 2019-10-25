using Microsoft.Extensions.DependencyInjection;

namespace DotnetMigrations.Lib
{
	public interface IMigrationsServicesBuilder
	{
		IServiceCollection Services { get; }
	}

	public class MigrationsServicesBuilder : IMigrationsServicesBuilder
	{
		public IServiceCollection Services { get; }

		public MigrationsServicesBuilder(IServiceCollection services)
		{
			Services = services;
		}
	}
}