using System;

namespace DotnetMigrations.Lib
{
    public static class Providers
    {
        public const string Npgsql = nameof(Npgsql);

        public const string Sqlite = nameof(Sqlite);

        public const string SqlServer = nameof(SqlServer);

        public const string Default = Npgsql;

        public const string AllString = Npgsql + ", " + SqlServer + ", " + Sqlite;

        public static bool IsProviderValid(string value)
        {
            return string.Compare(Npgsql, value, StringComparison.InvariantCultureIgnoreCase) == 0 ||
                   string.Compare(SqlServer, value, StringComparison.InvariantCultureIgnoreCase) == 0 ||
                   string.Compare(Sqlite, value, StringComparison.InvariantCultureIgnoreCase) == 0;
        }
    }
}
