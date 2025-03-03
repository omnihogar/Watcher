using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using ExitException;
using Dapper;
using System.ComponentModel.Design;

namespace DatabaseMode
{
    public class Databaser
    {
        private static string connectionString = "Data Source=sync_log.db";

        public static void FileManager()
        {
            Console.WriteLine("Database Mode - View File Changes");
            Console.WriteLine("Press m and enter to exit");
            Console.WriteLine("-----------------------------------");

            if (Console.ReadLine().ToLower() == "m")
            {
                throw new ThrowExitException();
            }

            List<FileRecord> records = GetFileRecords();

            if (records.Count == 0)
            {
                Console.WriteLine("No records found in the database.");
                return;
            }

            // Display files from the database
            Console.WriteLine("ID\tFile Name\t\tAction\t\tTimestamp");
            foreach (var record in records)
            {
                Console.WriteLine($"{record.Id}\t{record.FileName}\t{record.Action}\t{record.Timestamp}");
            }

            // Let user select a file to open
            Console.Write("\nEnter the ID of the file to open (or 0 to exit): ");
            if (int.TryParse(Console.ReadLine(), out int selectedId) && selectedId != 0)
            {
                FileRecord selectedRecord = records.FirstOrDefault(r => r.Id == selectedId);
                if (selectedRecord != null)
                {
                    OpenFile(selectedRecord.FileName);
                }
                else
                {
                    Console.WriteLine("Invalid file ID.");
                }
            }
        }

        private static List<FileRecord> GetFileRecords()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Id, FileName, Action, Timestamp FROM FileChanges ORDER BY Timestamp DESC";
                return connection.Query<FileRecord>(query).ToList();
            }
        }

        private static void OpenFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                    Console.WriteLine($"Opening: {filePath}");
                }
                else
                {
                    Console.WriteLine("File not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error opening file: " + ex.Message);
            }
        }
    }

    // Data model
    public class FileRecord
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
