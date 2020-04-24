using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureTableStorage
{    
    public class tablemanager
    {
        const string AZURESTORAGECONNECTIONSTRING = "DefaultEndpointsProtocol=https;AccountName=gravityrulestablestorage;AccountKey=O/+8r7qFwdpIJiDKbTlZQqJZtoaFqi8DD3RKUqo7Q8wcKFdGBu+2COxMGiQGVftT/zfw6h5rZvgyU0YfPqi/sA==;EndpointSuffix=core.windows.net";

        private CloudTable _table;

        public tablemanager(string tableName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(AZURESTORAGECONNECTIONSTRING);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            _table = tableClient.GetTableReference(tableName);
        }

        public async Task<TableResult> InsertEntity<T>(T entity) where T : TableEntity, new()
        {
            var op = TableOperation.Insert(entity);
            TableResult result = await _table.ExecuteAsync(op);

            return result;
        }

        public async Task<List<T>> RetreiveEntity<T>(string query = null) where T : TableEntity, new()
        {
            TableQuery<T> DataTableQuery = new TableQuery<T>();
            if (!String.IsNullOrEmpty(query))
            {
                DataTableQuery = new TableQuery<T>().Where(query);
            }
            TableContinuationToken ct = default(TableContinuationToken);

            var queryResult = await _table.ExecuteQuerySegmentedAsync(DataTableQuery, ct);

            return queryResult.Results;
        }
    }
}
