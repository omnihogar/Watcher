using ExitException;

namespace WatcherName
{
    public class Watcher
    {
        // Main klasa sadrži samo izbor modova
        static void Main()
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("\n=====================================");
                    Console.WriteLine("|      MAIN MENU - CHOOSE MODE      |");
                    Console.WriteLine("=====================================");
                    Console.WriteLine("| 1. Sync Mode        | Type: sync  |");
                    Console.WriteLine("| 2. Database Mode    | Type: data  |");
                    Console.WriteLine("| Return to Main Menu | Press 'm'   |");
                    Console.WriteLine("| Exit Program        | Type: exit  |");
                    Console.WriteLine("=====================================");
                    Console.Write("Choose mode: ");

                    string mode = Console.ReadLine().ToLower(); // Normalize input

                    if (mode == "sync")
                    {
                        SyncMode.Syncer.SyncStart();
                    }
                    else if (mode == "data")
                    {
                        DatabaseMode.Databaser.FileManager();
                    }
                    else if (mode == "exit")
                    {
                        Console.WriteLine("Exiting program...");
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Invalid mode. Try again.");
                    }
                }
                catch (ThrowExitException)
                {
                    Console.WriteLine("\nReturning to main menu...\n");
                }
            }
        }
    }
}
