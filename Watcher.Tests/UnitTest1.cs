using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using Xunit;
using Dapper;
using DatabaseMode;
using SyncMode;

namespace Watcher.Tests
{
    public class WatcherTests
    {
        [Fact]
        public void SyncStart_ShouldCopyFilesToDestination()
        {
            // Arrange
            string sourceDir = Path.Combine(Path.GetTempPath(), "SourceTestDir");
            string destinationDir = Path.Combine(Path.GetTempPath(), "DestinationTestDir");
            string testFile = Path.Combine(sourceDir, "testfile.txt");

            Directory.CreateDirectory(sourceDir);

            if (Directory.Exists(destinationDir))
            {
                Directory.Delete(destinationDir, true);
            }

            File.WriteAllText(testFile, "This is a test file");

            // Act
            Syncer.SyncStart();

            // Assert - Check if the test file exists in the destination directory
            string copiedFile = Path.Combine(destinationDir, "testfile.txt");
            Assert.True(File.Exists(copiedFile), "File was not copied to the destination directory.");

            // Cleanup
            Directory.Delete(sourceDir, true);
            Directory.Delete(destinationDir, true);
        }
    }

    public class DatabaseModeTests
    {
        private const string TestConnectionString = "Data Source=:memory:";

        [Fact]
        public void GetFileRecords_ShouldReturnInsertedRecords()
        {
            // Arrange
            using (var connection = new SqliteConnection(TestConnectionString))
            {
                connection.Open();

                // Create the table
                string createTable = "CREATE TABLE IF NOT EXISTS FileChanges (Id INTEGER PRIMARY KEY, FileName TEXT, Action TEXT, Timestamp DATETIME)";
                connection.Execute(createTable);

                // Insert test records
                string insertQuery = "INSERT INTO FileChanges (FileName, Action, Timestamp) VALUES (@FileName, @Action, @Timestamp)";
                connection.Execute(insertQuery, new { FileName = "test.txt", Action = "Created", Timestamp = DateTime.Now });

                // Act
                List<FileRecord> records = DatabaseHelper.GetFileRecords(connection);  // Corrected method call

                // Assert
                Assert.NotNull(records);
                Assert.Single(records);
                Assert.Equal("test.txt", records.First().FileName);
                Assert.Equal("Created", records.First().Action);
            }
        }
    }

    // Ensure this class exists in DatabaseMode
    public static class DatabaseHelper
    {
        public static List<FileRecord> GetFileRecords(SqliteConnection connection)
        {
            string query = "SELECT Id, FileName, Action, Timestamp FROM FileChanges ORDER BY Timestamp DESC";
            return connection.Query<FileRecord>(query).ToList();
        }
    }

    // Ensure this class exists in DatabaseMode
    public class FileRecord
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
    }
}