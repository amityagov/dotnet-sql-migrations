using System;
using System.Collections.Generic;
using System.Data.Common;
using DotnetMigrations.Lib.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace DotnetMigrations.Lib.SqliteProvider
{
	public class SqliteMigrationExecutor : MigrationExecutor<SqliteConnection>
	{
		public override string Type { get; } = Providers.Sqlite;

		public SqliteMigrationExecutor(ILogger<SqliteMigrationExecutor> logger)
			: base(logger)
		{
		}

		protected override DbCommand GetWriteMigrationAppliedDataCommand(MigrationInfo migrationInfo, DbConnection connection)
		{
			var command = connection.CreateCommand();

			var dateApplied = DateTime.UtcNow.ToString("o");

			command.CommandText = $"INSERT INTO \"{MigrationHistoryTableName}\"" +
								  $"(\"{nameof(MigrationInfo.Timestamp)}\"," +
								  $"\"{nameof(MigrationInfo.MigrationName)}\"," +
								  $"\"{nameof(MigrationInfo.Hash)}\"," +
								  $"\"{DateApplied}\")" +
								  $" VALUES('{migrationInfo.Timestamp}','{migrationInfo.MigrationName}', '{migrationInfo.Hash}', '{dateApplied}')";

			return command;
		}

		protected override IEnumerable<MigrationInfo> GetCurrentAppliedMigrations(DbConnection connection)
		{
			var command = connection.CreateCommand();

			var migrations = new List<MigrationInfo>();

			using (command)
			{
				command.CommandText = $"SELECT \"{nameof(MigrationInfo.Timestamp)}\"," +
									  $" \"{nameof(MigrationInfo.Hash)}\" FROM \"{MigrationHistoryTableName}\";";

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

		protected override void EnsureMigrationHistoryTableExists(DbConnection connection)
		{
			var command = connection.CreateCommand();
			command.CommandText = $"SELECT count(*) FROM sqlite_master WHERE type='table' AND name='{MigrationHistoryTableName}';";

			if (Convert.ToInt32(command.ExecuteScalar()) == 0)
			{
				Logger.LogInformation($"Create \"{MigrationHistoryTableName}\" table.");

				var createMigrationTableCommand = connection.CreateCommand();

				using (createMigrationTableCommand)
				{
					createMigrationTableCommand.CommandText = $"create table {MigrationHistoryTableName} (" +
															  $"{nameof(MigrationInfo.Hash)} TEXT not null constraint ___MigrationHistory_pk primary key," +
															  $"{nameof(MigrationInfo.MigrationName)} TEXT not null," +
															  $"{nameof(MigrationInfo.Timestamp)} text," +
															  $"{DateApplied} BLOB not null);";

					createMigrationTableCommand.ExecuteNonQuery();
				}
			}
		}
	}
}
