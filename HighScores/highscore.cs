using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace HighScores
{
    public class highscore : TableEntity
    {
        public int Score { get; set; }
        public string UserName { get; set; }

        public highscore()
        {
            RowKey = Guid.NewGuid().ToString();
        }
    }
}
