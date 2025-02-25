using System.Runtime.CompilerServices;

namespace Watcher.Tests
{
    public class WatcherTests
    {
        [Fact]
        public void Test1()
        {
            try
            {
                //Arrange
                string sourceDir = "C:\\Users\\Public\\test";
                string destinationDir = "C:\\Users\\Public\\test";
                //string connectionString = "Data Source=sync_log.db"; ;
                SyncMode syncTest = new SyncMode();

                //Act
                string result = syncTest.SyncStart(sourceDir, destinationDir);

                //Assert


            }

            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
