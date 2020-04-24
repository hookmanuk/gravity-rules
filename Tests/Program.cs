using AzureTableStorage;
using HighScores;
using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            MainAsync().Wait();            
        }

        static async Task MainAsync()
        {
            var ScoreManager = new scoremanager();             
            TableResult result = await ScoreManager.InsertScore(new highscore
            {
                PartitionKey = "highscore", //set to highscore partition only if in top 10
                RowKey = Guid.NewGuid().ToString(),
                Score = Convert.ToInt32(99),
                Timestamp = DateTimeOffset.UtcNow,
                UserName = "test"
            });
        }
    }
}
