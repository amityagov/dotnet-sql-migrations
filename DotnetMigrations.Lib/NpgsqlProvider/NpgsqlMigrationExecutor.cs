using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Transactions;
using DotnetMigrations.Lib.Models;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DotnetMigrations.Lib.NpgsqlProvider
{
	public class NpgsqlMigrationExecutor : IMigrationExecutor
	{
		public const string DateApplied = "DateApplied";

		private readonly ILogger<NpgsqlMigrationExecutor> _logger;

		private const string MigrationHistoryTableName = "___MigrationHistory";

		public string Type { get; } = Providers.Npgsql;

		public NpgsqlMigrationExecutor(ILogger<NpgsqlMigrationExecutor> logger)
		{
			_logger = logger;
		}

		private static DbConnection CreateConnection(string connectionString)
		{
			return new NpgsqlConnection(connectionString);
		}

		private static IEnumerable<MigrationInfo> GetCurrentAppliedMigrations(DbConnection connection)
		{
			var command = connection.CreateCommand();

			var migrations = new List<MigrationInfo>();

			using (command)
			{
				command.CommandText = $"SELECT \"{nameof(MigrationInfo.Timestamp)}\"," +
									  $" \"{nameof(MigrationInfo.Hash)}\" FROM public.\"{MigrationHistoryTableName}\";";

				var reader = command.ExecuteReader();

				using (reader)
				{
					while (reader.Read())
					{
						var timestamp = reader.GetString(0);
						var hash = reader.GetString(1);

						migrations.Add(new MigrationInfo(timestamp, hash));
					}
				}
			}

			return migrations.ToArray();
		}

		private void EnsureMigrationHistoryTableExists(DbConnection connection)
		{
			var command = connection.CreateCommand();
			command.CommandText = $"SELECT count(*) FROM pg_class WHERE relname='{MigrationHistoryTableName}';";

			if (Convert.ToInt32(command.ExecuteScalar()) == 0)
			{
				_logger.LogInformation($"Create \"{MigrationHistoryTableName}\" table.");

				var createMigrationTableCommand = connection.CreateCommand();

				using (createMigrationTableCommand)
				{
					createMigrationTableCommand.CommandText = $"CREATE TABLE public.\"{MigrationHistoryTableName}\"" +
															  "(" +
															  $"\"{nameof(MigrationInfo.Timestamp)}\" character varying(10) NOT NULL," +
															  $"\"{nameof(MigrationInfo.MigrationName)}\" character varying(128) NOT NULL," +
															  $"\"{nameof(MigrationInfo.Hash)}\" character varying(32) NOT NULL," +
															  $"\"{nameof(DateApplied)}\" timestamp without time zone NOT NULL default current_timestamp," +
															  $"CONSTRAINT \"PK_{MigrationHistoryTableName}\" PRIMARY KEY (\"Hash\")" +
															  ");";

					createMigrationTableCommand.ExecuteNonQuery();
				}
			}
		}

		public void Execute(string connectionString, IList<MigrationInfo> files, bool dryRun)
		{
			using (var scope = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(5)))
			{
				var connection = CreateConnection(connectionString);
				using (connection)
				{
					if (dryRun)
					{
						_logger.LogInformation("Start migrations. Dry run, transaction will be aborted.");
					}
					else
					{
						_logger.LogInformation("Start migrations.");
					}

					connection.Open();

					EnsureMigrationHistoryTableExists(connection);
					var appliedMigrations = GetCurrentAppliedMigrations(connection);

					var migrationsToApply = files.Except(appliedMigrations).ToArray();
					_logger.LogInformation($"Ready to apply {migrationsToApply.Length} migrations.");

					migrationsToApply = migrationsToApply.OrderBy(x => x.Timestamp).ToArray();

					try
					{
						foreach (var migrationInfo in migrationsToApply)
						{
							ApplyMigration(migrationInfo, connection);
							WriteMigrationAppliedData(migrationInfo, connection);
						}
					}
					catch (Exception)
					{
						_logger.LogError("Some errors occured. Rollback all changes and exit. Have a nice day.");
						throw;
					}

					if (dryRun == false)
					{
						scope.Complete();
					}

					connection.Close();

					_logger.LogInformation("Finish migrations.");
				}
			}
		}

		private void WriteMigrationAppliedData(MigrationInfo migrationInfo, DbConnection connection)
		{
			try
			{
				var command = connection.CreateCommand();

				using (command)
				{
					command.CommandText = $"INSERT INTO public.\"{MigrationHistoryTableName}\"" +
										  $"(\"{nameof(MigrationInfo.Timestamp)}\",\"{nameof(MigrationInfo.MigrationName)}\",\"{nameof(MigrationInfo.Hash)}\")" +
										  $" VALUES('{migrationInfo.Timestamp}','{migrationInfo.MigrationName}', '{migrationInfo.Hash}')";
					command.ExecuteNonQuery();
				}
			}
			catch (Exception e)
			{
				_logger.LogError($"Error while write migration data {migrationInfo.MigrationName} - {e.Message}.");
				throw;
			}
		}

		private void ApplyMigration(MigrationInfo migrationInfo, DbConnection connection)
		{
			try
			{
				var command = connection.CreateCommand();

				using (command)
				{
					command.CommandText = migrationInfo.Data;
					command.ExecuteNonQuery();

					_logger.LogInformation($"Migration {migrationInfo.MigrationName} applied.");
				}
			}
			catch (Exception e)
			{
				_logger.LogError($"Error while applying {migrationInfo.MigrationName} - {e.Message}.");
				throw;
			}
		}
	}
}
