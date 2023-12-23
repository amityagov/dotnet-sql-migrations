using System;
using Microsoft.Extensions.Options;

namespace DotnetMigrations.Lib
{
    internal class ConfigureMigrationRunnerOptions : IConfigureOptions<MigrationRunnerOptions>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Action<IServiceProvider, MigrationRunnerOptions> _action;

        public ConfigureMigrationRunnerOptions(IServiceProvider serviceProvider,
            Action<IServiceProvider, MigrationRunnerOptions> action)
        {
            _serviceProvider = serviceProvider;
            _action = action;
        }

        public void Configure(MigrationRunnerOptions options)
        {
            _action(_serviceProvider, options);
        }
    }
}
