using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace DotnetMigrations.Lib
{
	public interface IMigrationRunner
	{
		Task ExecuteAsync(CancellationToken cancellationToken);
	}

	public class MigrationRunner : IMigrationRunner
	{
		private readonly IOptions<MigrationRunnerOptions> _options;
		private readonly IProviderCollection _providerCollection;
		private readonly IMigrationFilesLoader _migrationFilesLoader;

		public MigrationRunner(IOptions<MigrationRunnerOptions> options,
			IProviderCollection providerCollection, IMigrationFilesLoader migrationFilesLoader)
		{
			_options = options;
			_providerCollection = providerCollection;
			_migrationFilesLoader = migrationFilesLoader;
		}

		public Task ExecuteAsync(CancellationToken cancellationToken)
		{
			var options = _options.Value;
			ValidateOptions(options);

			var migrationExecutor = _providerCollection.GetMigrationExecutor(options.ProviderType);

			var fileProviders = options.FileProviders;

			var migrations = _migrationFilesLoader.Load(fileProviders, options.Pattern);

			return migrationExecutor.ExecuteAsync(options.ConnectionString, migrations, options.DryRun, cancellationToken);
		}

		private static void ValidateOptions(MigrationRunnerOptions options)
		{
			Validator.ValidateObject(options, new ValidationContext(options));
		}
	}
}
