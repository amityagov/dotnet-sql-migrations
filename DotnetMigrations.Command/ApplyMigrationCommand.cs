using System.Threading;
using System.Threading.Tasks;
using DotnetMigrations.Lib;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetMigrations.Command
{
	[Command("migration")]
	public class ApplyMigrationCommand : IMigrationRunnerArguments
	{
		[Option("-p|--provider", "Database type. Supported providers: " + Providers.AllString, CommandOptionType.SingleValue)]
		public string Provider { get; set; }

		/// <summary>
		/// "-c|--config"
		/// "Path to config file, if not specified, default file \"migrations-config.json\" will be used (is exists)"
		/// </summary>
		[Option("-c|--config",
			"Path to config file, if not specified, default file \"migrations-config.json\" will be used (is exists). " +
			"If MIGRATIONS_ENVIRONMENT environment variable specified, config file with name \"previous-found-config.{migrations_environment_value}.json\". " +
			"For example migrations-config.json in \"Staging\" environment will be translated to migrations-config.staging.json.",
			CommandOptionType.SingleValue)]
		public string ConfigFilePath { get; set; }

		/// <summary>
		/// "-cs|--connectionString"
		/// "Connection string to database"
		/// </summary>
		[Option("-cs|--connectionString", "Connection string to database", CommandOptionType.SingleValue)]
		public string ConnectionString { get; set; }

		/// <summary>
		/// "-d|--migrationsDirectory"
		/// "Path to migrations directory"
		/// </summary>
		[Option("-d|--migrationsDirectory", "Path to migrations directories, comma separated", CommandOptionType.SingleValue)]
		public string MigrationsDirectory { get; set; }

		/// <summary>
		/// "-n|--connectionStringName"
		/// "ConnectionString name from config file, \"Default\" will be use if not specified"
		/// </summary>
		[Option("-n|--connectionStringName", "ConnectionString name from config file, \"Default\" will be use if not specified", CommandOptionType.SingleValue)]
		public string ConnectionStringName { get; set; }

		/// <summary>
		/// "-e|--environment"
		/// "Environment name to use, environment variable \"MIGRATIONS_ENVIRONMENT\" will overwrite this value is specified"
		/// </summary>
		[Option("-e|--environment", "Environment name to use, environment variable \"MIGRATIONS_ENVIRONMENT\" will overwrite this value is specified", CommandOptionType.SingleValue)]
		public string EnvironmentName { get; set; }

		/// <summary>
		/// "-dry|--dry"
		/// "Dry run, apply scripts without commit transaction"
		/// </summary>
		[Option("-dry|--dry", "Dry run, apply scripts without commit transaction", CommandOptionType.NoValue)]
		public bool DryRun { get; set; }

		public async Task<int> OnExecuteAsync(CommandLineApplication app, IConsole console, CancellationToken cancellationToken)
		{
			var runner = app.GetRequiredService<CommandMigrationRunner>();

			var result = await runner.ExecuteAsync(this, cancellationToken);

			if (result > 0)
			{
				app.ShowHelp();
			}

			return result;
		}
	}
}
