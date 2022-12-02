using DotnetMigrations.Lib.Models;
using Xunit;

namespace DotnetMigrations.Lib.Tests
{
	public class MigrationInfoTests
	{
		[Fact]
		public void AssertEqual()
		{
			var one = new MigrationInfo("timestamp", "test", "hash");
			var two = new MigrationInfo("timestamp", "test", "hash");

			Assert.Equal(one, two);
		}
	}
}
