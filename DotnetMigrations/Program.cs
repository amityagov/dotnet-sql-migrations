using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DotnetMigrations.Command;
using DotnetMigrations.Lib;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotnetMigrations8
{
	public class Program : ApplyMigrationCommand
	{
		public const string Name = "dotnet migration";

		public const string MigrationsStartDelaySecondsEnvironmentVariable = "MIGRATIONS_START_DELAY_SECONDS";

		public const string MigrationsInfiniteSleepEnvironmentVariable = "MIGRATIONS_INFINITE_SLEEP";

		public static async Task<int> Main(string[] args)
		{
			var startDelay = Environment.GetEnvironmentVariable(MigrationsStartDelaySecondsEnvironmentVariable);

			var collection = new ServiceCollection();
			collection.AddLogging();

			collection.AddLogging(builder =>
			{
				builder.AddConsole();
			});

			collection.AddMigrations().AddCommands();

			var services = collection.BuildServiceProvider();

			string VersionGetter() => Assembly.GetEntryAssembly().GetName().Version.ToString();

			var application = MigrationsApplication.Create<Program>(Name, VersionGetter, services);

			var logger = services.GetRequiredService<ILogger<Program>>();

			if (!string.IsNullOrEmpty(startDelay) && int.TryParse(startDelay, out var delay))
			{
				logger.LogInformation("Sleep for {Delay} seconds", delay);

				Thread.Sleep(TimeSpan.FromSeconds(delay));
			}

			int code;

			using (services)
			{
				try
				{
					code = await application.ExecuteAsync(args);
				}
				catch (UnrecognizedCommandParsingException e)
				{
					logger.LogError(e.Message, e);

					return 1;
				}
				catch (Exception e)
				{
					logger.LogError(e.GetType().FullName + ": " + e.Message, e);

					return 1;
				}
			}

			if (code == 0 && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(MigrationsInfiniteSleepEnvironmentVariable)))
			{
				var waitEvent = new ManualResetEventSlim(false);

				Console.CancelKeyPress += (sender, eventArgs) =>
				{
					eventArgs.Cancel = true;
					waitEvent.Set();
				};

				logger.LogInformation("Sleep for infinite time");
				waitEvent.Wait();
			}

			return code;
		}
	}
}
