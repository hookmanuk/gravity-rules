using HighScores;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureTableStorage
{    
    public class scoremanager : tablemanager
    {        
        public scoremanager() : base("highscore")
        {
        }

        public async Task<TableResult> InsertScore(highscore highscore)
        {
            TableResult result = await InsertEntity<highscore>(highscore);

            return result;
        }

        public async Task<List<highscore>> GetAllScores()
        {
            return await RetreiveEntity<highscore>("");                
        }
    }
}
