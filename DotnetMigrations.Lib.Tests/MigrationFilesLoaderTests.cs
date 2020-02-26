using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.FileProviders;
using Moq;
using Xunit;

namespace DotnetMigrations.Lib.Tests
{
	public class MigrationFilesLoaderTests
	{
		[Fact]
		public void Load()
		{
			var loader = new MigrationFilesLoader();

			var fileProviderMock = new Mock<IFileProvider>();

			var files = new TestableIDirectoryContents(Array.Empty<IFileInfo>());

			fileProviderMock.Setup(x => x.GetDirectoryContents(It.IsAny<string>())).Returns(() => files);

			var migrations = loader.Load(new List<IFileProvider> { fileProviderMock.Object }, "*.sql");

			Assert.NotNull(migrations);
			Assert.Empty(migrations);
		}

		[Fact]
		public void Load_one_file_with_valid_name_file_loaded()
		{
			var loader = new MigrationFilesLoader();

			var fileProviderMock = new Mock<IFileProvider>();

			var files = new TestableIDirectoryContents(new IFileInfo[] { new TestableFileInfo("2014010101 - create table.sql", "some sql", false) });

			fileProviderMock.Setup(x => x.GetDirectoryContents(It.IsAny<string>())).Returns(() => files);

			var migrations = loader.Load(new List<IFileProvider> { fileProviderMock.Object }, "*.sql");

			Assert.NotNull(migrations);
			Assert.NotEmpty(migrations);

			var migrationInfo = migrations.First();

			Assert.NotEmpty(migrationInfo.Data);
		}

		[Fact]
		public void Load_one_file_with_invalid_name_throws()
		{
			var loader = new MigrationFilesLoader();

			var fileProviderMock = new Mock<IFileProvider>();

			var files = new TestableIDirectoryContents(new IFileInfo[] { new TestableFileInfo("create table.sql", "some sql", false) });

			fileProviderMock.Setup(x => x.GetDirectoryContents(It.IsAny<string>())).Returns(() => files);

			Assert.Throws<InvalidOperationException>(() =>
			{
				var migrations = loader.Load(new List<IFileProvider> { fileProviderMock.Object }, "*.sql");
				Assert.NotNull(migrations);
				Assert.NotEmpty(migrations);
			});
		}

		public class TestableFileInfo : IFileInfo
		{
			private readonly string _data;

			public TestableFileInfo(string name, string data, bool isDirectory)
			{
				_data = data;
				Name = name;
				IsDirectory = isDirectory;
				Length = data.Length;
			}

			public Stream CreateReadStream()
			{
				var stream = new MemoryStream();

				var writer = new StreamWriter(stream);

				writer.Write(_data);

				writer.Flush();

				return stream;
			}

			public bool Exists { get; } = true;

			public long Length { get; }

			public string PhysicalPath { get; } = "";

			public string Name { get; }

			public DateTimeOffset LastModified { get; } = DateTimeOffset.Now;

			public bool IsDirectory { get; }
		}

		public class TestableIDirectoryContents : IDirectoryContents
		{
			private readonly IEnumerable<IFileInfo> _files;

			public TestableIDirectoryContents(IEnumerable<IFileInfo> files)
			{
				_files = files;
			}

			public IEnumerator<IFileInfo> GetEnumerator()
			{
				return _files.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public bool Exists { get; } = true;
		}
	}
}
