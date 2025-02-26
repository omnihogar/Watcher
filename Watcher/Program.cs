public class Watcher
{
    //Main klasa sadrži samo izbor modova
    static void Main()
    {
        Console.WriteLine("1. Sync mode");
        Console.WriteLine("2. Database mode");
        Console.Write("Choose mode: ");
        string mode = Console.ReadLine();
        if (mode == "1")
        {
            SyncMode.Syncer.SyncStart();
        }
        else if (mode == "2")
        {
            DatabaseMode.Databaser.FileManager();
        }
        else
        {
            Console.WriteLine("Invalid mode.");
        }
    }
}
