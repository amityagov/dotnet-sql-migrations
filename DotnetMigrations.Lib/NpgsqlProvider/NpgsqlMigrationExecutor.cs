using System;
using System.Collections.Generic;
using System.Data.Common;
using DotnetMigrations.Lib.Models;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DotnetMigrations.Lib.NpgsqlProvider
{
	public class NpgsqlMigrationExecutor : MigrationExecutor<NpgsqlConnection>
	{
		private readonly ILogger<NpgsqlMigrationExecutor> _logger;

		public override string Type { get; } = Providers.Npgsql;

		public NpgsqlMigrationExecutor(ILogger<NpgsqlMigrationExecutor> logger)
			: base(logger)
		{
			_logger = logger;
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

			return migrations;
		}

		protected override void EnsureMigrationHistoryTableExists(DbConnection connection)
		{
			var command = connection.CreateCommand();
			command.CommandText = $"SELECT count(*) FROM pg_class WHERE relname='{MigrationHistoryTableName}';";

			if (Convert.ToInt32(command.ExecuteScalar()) == 0)
			{
				_logger.LogInformation("Create \"{MigrationHistoryTableName}\" table", MigrationHistoryTableName);

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

		protected override DbCommand GetWriteMigrationAppliedDataCommand(MigrationInfo migrationInfo,
			DbConnection connection)
		{
			var command = connection.CreateCommand();

			command.CommandText = $"INSERT INTO public.\"{MigrationHistoryTableName}\"" +
			                      $"(\"{nameof(MigrationInfo.Timestamp)}\",\"{nameof(MigrationInfo.MigrationName)}\",\"{nameof(MigrationInfo.Hash)}\")" +
			                      $" VALUES('{migrationInfo.Timestamp}','{migrationInfo.MigrationName}', '{migrationInfo.Hash}')";

			return command;
		}
	}
}
