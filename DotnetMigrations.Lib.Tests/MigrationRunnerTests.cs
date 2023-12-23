using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotnetMigrations.Lib.Models;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace DotnetMigrations.Lib.Tests;

public class MigrationRunnerTests
{
    [Fact]
    public async Task ExecuteAsync()
    {
        var optionsWrapper = new OptionsWrapper<MigrationRunnerOptions>(new MigrationRunnerOptions
        {
            FileProviders = new List<IFileProvider> { new NullFileProvider() },
            ConnectionString = "Connection string"
        });

        var migrationFilesLoaderMock = new Mock<IMigrationFilesLoader>();

        migrationFilesLoaderMock.Setup(x => x.Load(It.IsAny<ICollection<IFileProvider>>(), string.Empty))
            .Returns(() => new List<MigrationInfo>());

        var migrationExecutorMock = new Mock<IMigrationExecutor>();

        migrationExecutorMock.Setup(x => x.Type).Returns(() => Providers.Npgsql);

        var migrationRunner = new MigrationRunner(optionsWrapper,
            new ProviderCollection(new[] { migrationExecutorMock.Object }),
            migrationFilesLoaderMock.Object);

        await migrationRunner.ExecuteAsync(CancellationToken.None);
    }
}
