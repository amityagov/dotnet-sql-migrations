using System;
using System.Reflection;
using System.Threading;
using DotnetMigrations.Lib;
using DotnetMigrations.Lib.NpgsqlProvider;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotnetMigrations8
{
	public class Program : ApplyMigrationCommand
	{
		public const string Name = "dotnet migration";

		public const string MigrationsStartDelaySecondsEnvironmentVariable = "MIGRATIONS_START_DELAY_SECONDS";

		public const string MigrationsInfiniteSleepEnvironmentVariable = "MIGRATIONS_INFINITE_SLEEP";

		public static int Main(string[] args)
		{
			var startDelay = Environment.GetEnvironmentVariable(MigrationsStartDelaySecondsEnvironmentVariable);

			var collection = new ServiceCollection();
			collection.AddLogging();

			collection.AddLogging(builder =>
			{
				builder.AddConsole();
			});

			collection.AddMigrations()
				.AddNpgsqlMigrations();// .AddSqlServerMigrations();

			var services = collection.BuildServiceProvider();

			string VersionGetter() => Assembly.GetEntryAssembly().GetName().Version.ToString();

			var application = MigrationsApplication.Create<Program>(Name, VersionGetter, services);

			var logger = services.GetRequiredService<ILogger<Program>>();

			if (!string.IsNullOrEmpty(startDelay) && int.TryParse(startDelay, out var delay))
			{
				logger.LogInformation($"Sleep for {delay} seconds.");

				Thread.Sleep(TimeSpan.FromSeconds(delay));
			}

			int code;

			using (services)
			{
				try
				{
					code = application.Execute(args);
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

				logger.LogInformation("Sleep for infinite time.");
				waitEvent.Wait();
			}

			return code;
		}
	}
}