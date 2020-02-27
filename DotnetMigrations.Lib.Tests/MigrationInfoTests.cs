using DotnetMigrations.Lib.Models;
using Xunit;

namespace DotnetMigrations.Lib.Tests
{
	public class MigrationInfoTests
	{
		[Fact]
		public void AssertEqual()
		{
			var one = new MigrationInfo("timestamp", "hash");
			var two = new MigrationInfo("timestamp", "hash");

			Assert.Equal(one, two);
		}

		[Fact]
		public void AssertEqualWithComparer()
		{
			var one = new MigrationInfo("timestamp", "hash");
			var two = new MigrationInfo("timestamp", "hash");

			Assert.True(MigrationInfo.TimestampComparer.Equals(one, two));
		}
	}
}
