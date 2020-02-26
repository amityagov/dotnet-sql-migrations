# dotnet-sql-migrations

**Experimental project**

![](https://github.com/amityagov/dotnet-sql-migrations/workflows/build/badge.svg)
![https://www.nuget.org/packages/DotnetMigrations8/](https://img.shields.io/nuget/dt/DotnetMigrations8)

## DotnetMigrations8
[Nuget](https://www.nuget.org/packages/DotnetMigrations8)

[.NET Core tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools) for running migrations from command line.

## DotnetMigrations8.Lib
[Nuget](https://www.nuget.org/packages/DotnetMigrations8.Lib)

Library for running migrations.

Startup.cs
``` csharp

public override void ConfigureServices(IServiceCollection collection)
{
    collection.AddMigrations((provider, options) =>
    {
        var configuration = provider.GetRequiredService<IConfiguration>();
        var environment = provider.GetRequiredService<IHostEnvironment>();

        var connectionString = configuration.GetConnectionString("Default");

        options.DryRun = false; // do not commit transaction after migrations applied, default = false
        options.Pattern = "*.sql"; // default = "*.sql";

        // Supported providers
        options.ProviderType = Providers.Npgsql;
        // options.ProviderType = Providers.SqlServer;

        options.ConnectionString = connectionString;
        options.FileProviders = new IFileProvider[]
        {
            new PhysicalFileProvider(Path.Combine(environment.ContentRootPath, "sql")), // folder
            new EmbeddedFileProvider(GetType().Assembly, "SqlResourcesNamespace") // embedded sql files
        };
    });

    ...
}
```

Program.cs
``` csharp
var host = hostBuilder.Build();

var migrationRunner = host.Services.GetRequiredService<IMigrationRunner>(); // Get service...

await migrationRunner.ExecuteAsync(CancellationToken.None); // ...and run migrations

await host.RunAsync();
```

## DotnetMigrations8.Command
[Nuget](https://www.nuget.org/packages/DotnetMigrations8.Command)

Migration command for [https://github.com/natemcmaster/CommandLineUtils](https://github.com/natemcmaster/CommandLineUtils).

See [ApplyMigrationCommand.cs](./DotnetMigrations.Command/ApplyMigrationCommand.cs).
