namespace DotnetMigrations.Lib
{
	public interface IMigrationRunnerArguments
	{
		/// <summary>
		/// "-c|--config"
		/// "Path to config file, if not specified, default file \"migrations-config.json\" will be used (is exists)"
		/// </summary>
		string ConfigFilePath { get; }

		/// <summary>
		/// "-cs|--connectionString"
		/// "Connection string to database"
		/// </summary>
		string ConnectionString { get; }

		/// <summary>
		/// "-d|--migrationsDirectory"
		/// "Path to migrations directory"
		/// </summary>
		string MigrationsDirectory { get; }

		/// <summary>
		/// "-n|--connectionStringName"
		/// "ConnectionString name from config file, \"Default\" will be use if not specified"
		/// </summary>
		string ConnectionStringName { get; }

		/// <summary>
		/// "-e|--environment"
		/// "Environment name to use, environment variable \"MIGRATIONS_ENVIRONMENT\" will overwrite this value is specified"
		/// </summary>
		string EnvironmentName { get; }

		/// <summary>
		/// "-dry|--dry"
		/// "Dry run, apply scripts without commit transaction"
		/// </summary>
		bool DryRun { get; }
	}
}