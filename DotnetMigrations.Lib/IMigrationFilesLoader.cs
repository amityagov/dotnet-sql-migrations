using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotnetMigrations.Lib.Models;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileSystemGlobbing;

namespace DotnetMigrations.Lib
{
	public interface IMigrationFilesLoader
	{
		ICollection<MigrationInfo> Load(ICollection<IFileProvider> fileProviders, string pattern);
	}

	public class MigrationFilesLoader : IMigrationFilesLoader
	{
		private readonly Regex _filePattern =
			new Regex(@"^(\d{10})\s*-\s*.*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public ICollection<MigrationInfo> Load(ICollection<IFileProvider> fileProviders, string pattern)
		{
			var matcher = new Matcher(StringComparison.OrdinalIgnoreCase);

			matcher.AddIncludePatterns(new[] { pattern });

			return fileProviders.SelectMany(x => LoadDirectory(x, matcher)).ToArray();
		}

		private IEnumerable<MigrationInfo> LoadDirectory(IFileProvider fileProvider, Matcher matcher)
		{
			var content = fileProvider.GetDirectoryContents(string.Empty);

			foreach (var fileInfo in content)
			{
				if (!fileInfo.Exists)
				{
					continue;
				}

				if (fileInfo.IsDirectory)
				{
					continue;
				}

				var patternMatchingResult = matcher.Match(fileInfo.Name);

				if (patternMatchingResult.HasMatches)
				{
					var match = _filePattern.Match(fileInfo.Name);

					if (match.Groups.Count < 2)
					{
						throw new InvalidOperationException($"File \"{fileInfo.Name}\" has invalid name format.");
					}

					var timestamp = match.Groups[1].Value;

					using var reader = new StreamReader(fileInfo.CreateReadStream());

					yield return new MigrationInfo(timestamp, fileInfo.Name,
						HashHelper.CalculateHash(content + "_" + timestamp)) { Data = reader.ReadToEnd() };
				}
			}
		}
	}
}
