using System;
using System.IO;
using System.Data;
using Microsoft.Data.Sqlite;
using Dapper;
using System.Text;
using ExitException;

namespace SyncMode
{
    public static class Syncer
    {
        private static string sourceDir;
        private static string destinationDir;
        private static string connectionString = "Data Source=sync_log.db";

        public static void SyncStart()
        {
            Console.WriteLine("Synchronisation mode - synchronize files and directories.");
            Console.WriteLine("Press m and enter to exit.");
            Console.WriteLine("-----------------------------------");

            if (Console.ReadLine().ToLower() == "m")
            {
                throw new ThrowExitException();
            }

            //Source
            Console.WriteLine("Use this format: C:\\Users\\Documents\\source");
            Console.Write("Enter source directory: ");
            string input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Invalid directory path.");
                return;
            }

            try
            {
                sourceDir = Path.GetFullPath(input);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Invalid source directory path: " + ex.Message);
                return;
            }

            if (!Directory.Exists(sourceDir))
            {
                Console.WriteLine("Source directory does not exist.");
                return;
            }

            // Destination
            Console.Write("Enter destination directory: ");
            input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Invalid directory path.");
                return;
            }

            try
            {
                destinationDir = Path.GetFullPath(input);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Invalid destination directory path: " + ex.Message);
                return;
            }

            if (!Directory.Exists(destinationDir))
            {
                Console.WriteLine("Destination directory does not exist.");
                return;
            }

            Console.WriteLine("Starting directory synchronization...");
            EnsureDatabaseExists();
            SyncDirectories(sourceDir, destinationDir);

            FileSystemWatcher watcher = new FileSystemWatcher(sourceDir);

            watcher.Created += OnCreated;
            watcher.Changed += OnChanged;
            watcher.Deleted += OnDeleted;
            watcher.Renamed += OnRenamed;

            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        //repliciranje promjena u direktoriju i na file-u
        private static void SyncDirectories(string source, string destination)
        {
            foreach (var dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
            {
                string destDirPath = dirPath.Replace(source, destination);
                if (!Directory.Exists(destDirPath))
                {
                    Directory.CreateDirectory(destDirPath);
                }
            }

            foreach (var filePath in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                string destFilePath = filePath.Replace(source, destination);
                File.Copy(filePath, destFilePath, true);
            }
        }

        //novi file je napravljen
        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            string destPath = e.FullPath.Replace(sourceDir, destinationDir);
            File.Copy(e.FullPath, destPath, true);
            LogChange(e.FullPath, "Created"); // FIXED: Use FullPath instead of just Name
            Console.WriteLine($"Created: {e.FullPath}");
        }

        //file je promijenjen
        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            string destPath = e.FullPath.Replace(sourceDir, destinationDir);
            if (Directory.Exists(e.FullPath))
            {
                Directory.CreateDirectory(destPath);
            }
            else
            {
                File.Copy(e.FullPath, destPath, true);
            }
            LogChange(e.FullPath, e.ChangeType.ToString()); // FIXED: Use FullPath
            Console.WriteLine($"{e.ChangeType}: {e.FullPath}");
        }

        //file je obrisan
        private static void OnDeleted(object sender, FileSystemEventArgs e)
        {
            string destPath = e.FullPath.Replace(sourceDir, destinationDir);
            if (Directory.Exists(destPath))
            {
                Directory.Delete(destPath, true);
            }
            else if (File.Exists(destPath))
            {
                File.Delete(destPath);
            }
            LogChange(e.FullPath, "Deleted"); // FIXED: Use FullPath
            Console.WriteLine($"Deleted: {e.FullPath}");
        }

        //file je preimenovan
        private static void OnRenamed(object sender, RenamedEventArgs e)
        {
            string oldDestPath = e.OldFullPath.Replace(sourceDir, destinationDir);
            string newDestPath = e.FullPath.Replace(sourceDir, destinationDir);

            if (Directory.Exists(oldDestPath))
            {
                Directory.Move(oldDestPath, newDestPath);
            }
            else if (File.Exists(oldDestPath))
            {
                File.Move(oldDestPath, newDestPath);
            }
            LogChange(e.OldFullPath, $"Renamed to {e.FullPath}"); // FIXED: Use FullPath
            Console.WriteLine($"Renamed: {e.OldFullPath} -> {e.FullPath}");
        }

        //izrada tablice i batabase file-a
        private static void EnsureDatabaseExists()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string sql = "CREATE TABLE IF NOT EXISTS FileChanges (Id INTEGER PRIMARY KEY, FileName TEXT, Extension TEXT, Size INTEGER, Action TEXT, Timestamp DATETIME)";
                connection.Execute(sql);
            }
        }

        //upisivanje promjena u bazu podataka
        private static void LogChange(string filePath, string action) // FIXED: Changed parameter name to reflect actual path
        {
            string absolutePath = Path.GetFullPath(filePath);  // FIXED: Ensure absolute path is stored
            string extension = Path.GetExtension(absolutePath);
            long size = File.Exists(absolutePath) ? new FileInfo(absolutePath).Length : 0;

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string sql = "INSERT INTO FileChanges (FileName, Extension, Size, Action, Timestamp) VALUES (@FileName, @Extension, @Size, @Action, @Timestamp)";
                connection.Execute(sql, new { FileName = absolutePath, Extension = extension, Size = size, Action = action, Timestamp = DateTime.Now });
            }
        }
    }
}
