using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using DotnetMigrations.Lib.Models;
using Microsoft.Extensions.Logging;

namespace DotnetMigrations.Lib
{
    public abstract class MigrationExecutor<TConnection> : IMigrationExecutor
        where TConnection : DbConnection, new()
    {
        protected const string DateApplied = "DateApplied";
        protected const string MigrationHistoryTableName = "___MigrationHistory";

        protected readonly ILogger Logger;

        public abstract string Type { get; }

        protected MigrationExecutor(ILogger logger)
        {
            Logger = logger;
        }

        private static DbConnection CreateConnection(string connectionString)
        {
            var connection = new TConnection { ConnectionString = connectionString };

            return connection;
        }

        protected abstract DbCommand GetWriteMigrationAppliedDataCommand(MigrationInfo migrationInfo,
            DbConnection connection);

        private void WriteMigrationAppliedData(MigrationInfo migrationInfo, DbConnection connection)
        {
            try
            {
                var command = GetWriteMigrationAppliedDataCommand(migrationInfo, connection);

                using (command)
                {
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error while write migration data {MigrationName} - {Message}",
                    migrationInfo.MigrationName, e.Message);

                throw;
            }
        }

        public async Task ExecuteAsync(string connectionString, ICollection<MigrationInfo> files, bool dryRun,
            CancellationToken cancellationToken)
        {
            using var scope = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(5),
                TransactionScopeAsyncFlowOption.Enabled);

            var connection = CreateConnection(connectionString);

            await using (connection)
            {
                if (dryRun)
                {
                    Logger.LogInformation("Start migrations. Dry run, transaction will be aborted");
                }
                else
                {
                    Logger.LogInformation("Start migrations");
                }

                await connection.OpenAsync(cancellationToken);

                EnsureMigrationHistoryTableExists(connection);
                IEnumerable<MigrationInfo> appliedMigrations = GetCurrentAppliedMigrations(connection);

                var migrationsToApply = files.Except(appliedMigrations, MigrationInfo.MigrationNameComparer).ToArray();

                Logger.LogInformation(
                    "Ready to apply {MigrationsCount} migrations, total migrations found {TotalMigrations}",
                    migrationsToApply.Length, files.Count);

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
                    Logger.LogError("Some errors occured. Rollback all changes and exit. Have a nice day");
                    throw;
                }

                if (dryRun == false)
                {
                    scope.Complete();
                }

                await connection.CloseAsync();

                Logger.LogInformation("Finish migrations");
            }
        }

        private async Task ApplyMigration(MigrationInfo migrationInfo, DbConnection connection,
            CancellationToken cancellationToken)
        {
            try
            {
                var command = connection.CreateCommand();
                command.CommandTimeout = 300;

                await using (command)
                {
                    command.CommandText = migrationInfo.Data;
                    await command.ExecuteNonQueryAsync(cancellationToken);

                    Logger.LogInformation("Migration \"{MigrationName}\" applied", migrationInfo.MigrationName);
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Error while applying \"{MigrationName}\" - {ErrorMessage}",
                    migrationInfo.MigrationName,
                    e.Message);
                throw;
            }
        }

        protected abstract IEnumerable<MigrationInfo> GetCurrentAppliedMigrations(DbConnection connection);

        protected abstract void EnsureMigrationHistoryTableExists(DbConnection connection);
    }
}
