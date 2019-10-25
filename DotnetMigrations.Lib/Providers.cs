namespace DotnetMigrations.Lib
{
	public class Providers
	{
		public const string Npgsql = nameof(Npgsql);

		public const string SqlServer = nameof(SqlServer);

		public const string Default = Npgsql;

		public static string[] All =
		{
			Npgsql,
			SqlServer
		};

		public const string AllString = Npgsql + ", " + SqlServer;
	}
}
