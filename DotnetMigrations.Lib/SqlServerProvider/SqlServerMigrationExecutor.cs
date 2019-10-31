using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using DotnetMigrations.Lib.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace DotnetMigrations.Lib.SqlServerProvider
{
	public class SqlServerMigrationExecutor : IMigrationExecutor
	{
		public const string DateApplied = "DateApplied";

		private readonly ILogger _logger;

		private const string MigrationHistoryTableName = "___MigrationHistory";

		public string Type { get; } = Providers.SqlServer;

		public SqlServerMigrationExecutor(ILogger<SqlServerMigrationExecutor> logger)
		{
			_logger = logger;
		}

		private static DbConnection CreateConnection(string connectionString)
		{
			return new SqlConnection(connectionString);
		}

		private static IEnumerable<MigrationInfo> GetCurrentAppliedMigrations(DbConnection connection)
		{
			var command = connection.CreateCommand();

			var migrations = new List<MigrationInfo>();

			using (command)
			{
				command.CommandText = $"SELECT {nameof(MigrationInfo.Timestamp)}," +
									  $" {nameof(MigrationInfo.Hash)} FROM {MigrationHistoryTableName};";

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
			command.CommandText = $"SELECT 1 where OBJECT_ID(N'{MigrationHistoryTableName}', N'U') is not null;";

			if (Convert.ToInt32(command.ExecuteScalar()) == 0)
			{
				_logger.LogInformation($"Create \"{MigrationHistoryTableName}\" table.");

				var createMigrationTableCommand = connection.CreateCommand();

				using (createMigrationTableCommand)
				{
					createMigrationTableCommand.CommandText = $@"CREATE TABLE [{MigrationHistoryTableName}] (
																[{nameof(MigrationInfo.Hash)}] [nvarchar](32) NOT NULL,
																[{nameof(MigrationInfo.Timestamp)}] [nvarchar](10) NOT NULL,
																[{nameof(MigrationInfo.MigrationName)}] [nvarchar](128) NOT NULL,
																[{nameof(DateApplied)}] [datetime] NOT NULL DEFAULT (getdate()),
															CONSTRAINT [PK_{MigrationHistoryTableName}] PRIMARY KEY({nameof(MigrationInfo.Hash)}));";

					createMigrationTableCommand.ExecuteNonQuery();
				}
			}
		}

		public async Task ExecuteAsync(string connectionString, IList<MigrationInfo> files, bool dryRun,
			CancellationToken cancellationToken)
		{
			using (var scope = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(5), TransactionScopeAsyncFlowOption.Enabled))
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
							await ApplyMigration(migrationInfo, connection, cancellationToken);
							WriteMigrationAppliedData(migrationInfo, connection);
						}
					}
					catch (Exception)
					{
						_logger.LogError("Some errors occured. Rollback all changes and exit. Have a nice day.");
						throw;
					}

					connection.Close();

					if (dryRun == false)
					{
						scope.Complete();
					}

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
					command.CommandText = $"INSERT INTO \"{MigrationHistoryTableName}\"" +
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

		private async Task ApplyMigration(MigrationInfo migrationInfo, DbConnection connection,
			CancellationToken cancellationToken)
		{
			try
			{
				var sw = new Stopwatch();
				sw.Start();

				var command = connection.CreateCommand();
				using (command)
				{
					command.CommandText = migrationInfo.Data;

					await command.ExecuteNonQueryAsync(cancellationToken);

					sw.Stop();

					_logger.LogInformation($"Migration {migrationInfo.MigrationName} applied in {sw.Elapsed}.");
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
