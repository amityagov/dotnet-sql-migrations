using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace DotnetMigrations.Lib
{
	public class MigrationRunner
	{
		private readonly ILogger _logger;
		private readonly IMigrationExecutor _executor;
		private readonly MigrationOptionsLoader _migrationOptionsLoader;

		public const string Pattern = "*.sql";

		private readonly Regex _filePattern = new Regex(@"^(\d{10})\s*-\s*.*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public MigrationRunner(ILogger<MigrationRunner> logger, IMigrationExecutor executor, MigrationOptionsLoader migrationOptionsLoader)
		{
			_logger = logger;
			_executor = executor;
			_migrationOptionsLoader = migrationOptionsLoader;
		}

		public int Run(IMigrationRunnerArguments arguments)
		{
			var configFilePath = arguments.ConfigFilePath;
			var connectionString = arguments.ConnectionString;
			var migrationsDirectory = arguments.MigrationsDirectory;
			var connectionStringName = arguments.ConnectionStringName;
			var environmentName = arguments.EnvironmentName;

			var environmentVariables = EnvironmentVariables.GetValues();

			try
			{
				if (_migrationOptionsLoader.TryGetOptions(configFilePath,
					connectionString,
					migrationsDirectory,
					connectionStringName,
					environmentName,
					environmentVariables,
					out var options))
				{
					var files = new List<MigrationInfo>();

					connectionString = options.ConnectionString;
					migrationsDirectory = options.MigrationsDirectory;

					if (ValidateFiles(migrationsDirectory, files))
					{
						_executor.Execute(connectionString, files, arguments.DryRun);
					}

					return 0;
				}

				return 1;
			}
			catch (Exception e)
			{
				_logger.LogError($"Error in migration process: {e.GetType()} - {e.Message}", e);

				return -1;
			}
		}

		private bool ValidateFiles(string directoryPath, IList<MigrationInfo> files)
		{
			_logger.LogInformation($"Use migrations directory: \"{directoryPath}\".");

			var directory = new DirectoryInfo(directoryPath);

			bool success = true;

			foreach (var file in directory.GetFiles(Pattern))
			{
				var info = ValidateFileName(file);

				if (info == null)
				{
					success = false;
					continue;
				}

				files.Add(info);
			}

			var duplicateTimestamps = files.GroupBy(x => x.Timestamp).Where(x => x.Count() > 1).Select(x => x.Key).ToArray();
			if (duplicateTimestamps.Any())
			{
				foreach (var timestamp in duplicateTimestamps)
				{
					_logger.LogWarning($"Duplicate timestamp: {timestamp}");
				}

				_logger.LogError("Duplicate timestamps found. Please fix. Exit.");
				return false;
			}

			if (!success)
			{
				_logger.LogError("Some files does not match suggested migration file pattern \"YYYYMMDDNN - migration name\". Exit.");
			}

			return success;
		}

		private MigrationInfo ValidateFileName(FileSystemInfo fileInfo)
		{
			var fileName = fileInfo.Name;
			var match = _filePattern.Match(fileName);

			if (match.Groups.Count < 2)
			{
				_logger.LogError($"File \"{fileName}\" has invalid name format.");
				return null;
			}

			var timestamp = match.Groups[1].Value;

			var content = File.ReadAllText(fileInfo.FullName);

			return new MigrationInfo(timestamp, HashHelper.CalculateHash(content + "_" + timestamp))
			{
				Data = content,
				MigrationName = fileName
			};
		}
	}
}
