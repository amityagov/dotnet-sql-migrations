using System;
using System.Collections.Generic;
using System.Data.Common;
using DotnetMigrations.Lib.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace DotnetMigrations.Lib.SqlServerProvider
{
	public class SqlServerMigrationExecutor : MigrationExecutor<SqlConnection>
	{
		private readonly ILogger<SqlServerMigrationExecutor> _logger;

		public override string Type { get; } = Providers.SqlServer;

		public SqlServerMigrationExecutor(ILogger<SqlServerMigrationExecutor> logger) : base(logger)
		{
			_logger = logger;
		}

		protected override DbCommand GetWriteMigrationAppliedDataCommand(MigrationInfo migrationInfo, DbConnection connection)
		{
			var command = connection.CreateCommand();

			command.CommandText = $"INSERT INTO \"{MigrationHistoryTableName}\"" +

								  $"(\"{nameof(MigrationInfo.Timestamp)}\"," +
								  $"\"{nameof(MigrationInfo.MigrationName)}\"," +
								  $"\"{nameof(MigrationInfo.Hash)}\")" +

								  $" VALUES('{migrationInfo.Timestamp}'," +
								  $"'{migrationInfo.MigrationName}'," +
								  $"'{migrationInfo.Hash}')";

			return command;
		}

		protected override IEnumerable<MigrationInfo> GetCurrentAppliedMigrations(DbConnection connection)
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

			return migrations;
		}

		protected override void EnsureMigrationHistoryTableExists(DbConnection connection)
		{
			var command = connection.CreateCommand();
			command.CommandText = $"SELECT 1 WHERE OBJECT_ID(N'{MigrationHistoryTableName}', N'U') IS NOT NULL;";

			if (Convert.ToInt32(command.ExecuteScalar()) == 0)
			{
				_logger.LogInformation("Create \"{MigrationHistoryTableName}\" table", MigrationHistoryTableName);

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
	}
}
