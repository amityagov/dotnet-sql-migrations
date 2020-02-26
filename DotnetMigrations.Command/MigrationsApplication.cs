using System;
using McMaster.Extensions.CommandLineUtils;

namespace DotnetMigrations.Command
{
	public static class MigrationsApplication
	{
		public static CommandLineApplication<T> Create<T>(string name, Func<string> versionGetter,
			IServiceProvider services) where T : class
		{
			var application = new CommandLineApplication<T>(false)
			{
				Name = name,
				FullName = name,
				ShortVersionGetter = versionGetter,
				ExtendedHelpText = Help.GetHelpText()
			};

			application.VersionOption("-v|--version", versionGetter);
			application.HelpOption("-h|--help");

			application.Conventions
				.UseDefaultConventions()
				.UseConstructorInjection(services);

			return application;
		}
	}
}
