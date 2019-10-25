namespace DotnetMigrations.Lib
{
	public class MigrationInfo
	{
		public string Timestamp { get; }

		public string Hash { get; }

		public string Data { get; set; }

		public string MigrationName { get; set; }

		public MigrationInfo(string timestamp, string hash)
		{
			Timestamp = timestamp;
			Hash = hash;
		}

		protected bool Equals(MigrationInfo other)
		{
			return string.Equals(Timestamp, other.Timestamp);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((MigrationInfo)obj);
		}

		public override int GetHashCode()
		{
			return (Timestamp != null ? Timestamp.GetHashCode() : 0);
		}

		public static bool operator ==(MigrationInfo left, MigrationInfo right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(MigrationInfo left, MigrationInfo right)
		{
			return !Equals(left, right);
		}
	}
}
