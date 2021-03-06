﻿using System.Collections.Generic;

namespace DotnetMigrations.Lib.Models
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

		private sealed class TimestampEqualityComparer : IEqualityComparer<MigrationInfo>
		{
			public bool Equals(MigrationInfo x, MigrationInfo y)
			{
				if (ReferenceEquals(x, y))
				{
					return true;
				}

				if (ReferenceEquals(x, null))
				{
					return false;
				}

				if (ReferenceEquals(y, null))
				{
					return false;
				}

				if (x.GetType() != y.GetType())
				{
					return false;
				}

				return x.Timestamp == y.Timestamp;
			}

			public int GetHashCode(MigrationInfo obj)
			{
				return (obj.Timestamp != null ? obj.Timestamp.GetHashCode() : 0);
			}
		}

		public static IEqualityComparer<MigrationInfo> TimestampComparer { get; } = new TimestampEqualityComparer();
	}
}
