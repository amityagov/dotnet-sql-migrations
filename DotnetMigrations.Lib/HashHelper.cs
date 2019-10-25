using System.Security.Cryptography;
using System.Text;

namespace DotnetMigrations.Lib
{
	public static class HashHelper
	{
		public static string CalculateHash(string data)
		{
			var md5 = MD5.Create();
			var bytes = Encoding.UTF8.GetBytes(data);

			var hash = md5.ComputeHash(bytes, 0, bytes.Length);

			var hex = new StringBuilder(hash.Length * 2);

			foreach (byte b in hash)
				hex.AppendFormat("{0:x2}", b);

			return hex.ToString();
		}
	}
}