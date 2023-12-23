using System.Collections.Generic;

namespace DotnetMigrations.Lib.Models
{
    public class MigrationInfo
    {
        public string Timestamp { get; }

        public string Hash { get; }

        public string Data { get; set; }

        public string MigrationName { get; }

        public MigrationInfo(string timestamp, string migrationName, string hash)
        {
            Timestamp = timestamp;
            MigrationName = migrationName;
            Hash = hash;
        }

        private bool Equals(MigrationInfo other)
        {
            return MigrationName == other.MigrationName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((MigrationInfo)obj);
        }

        public override int GetHashCode()
        {
            return (MigrationName != null ? MigrationName.GetHashCode() : 0);
        }

        private sealed class MigrationNameEqualityComparer : IEqualityComparer<MigrationInfo>
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

                return x.MigrationName == y.MigrationName;
            }

            public int GetHashCode(MigrationInfo obj)
            {
                return (obj.MigrationName != null ? obj.MigrationName.GetHashCode() : 0);
            }
        }

        public static IEqualityComparer<MigrationInfo> MigrationNameComparer { get; } =
            new MigrationNameEqualityComparer();
    }
}
