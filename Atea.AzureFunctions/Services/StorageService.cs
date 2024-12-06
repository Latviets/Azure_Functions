using Azure.Data.Tables;
using Microsoft.Extensions.Logging;

namespace Atea.AzureFunctions.Services
{
    public class StorageService
    {
        private readonly string _connectionString;
        private readonly ILogger _logger;
        private readonly string _storageTableName;

        public StorageService(string connectionString, ILogger logger, string storageTableName)
        {
            _connectionString = connectionString;
            _logger = logger;      
            _storageTableName = storageTableName;
        }

        public async Task SaveDataToTableStorageAsync(string partitionKey, string rowKey, HttpResponseMessage response)
        {
            var tableServiceClient = new TableServiceClient(_connectionString);
            await tableServiceClient.CreateTableIfNotExistsAsync(_storageTableName);
            var tableClient = tableServiceClient.GetTableClient(_storageTableName);

            var tableEntity = new TableEntity(response.IsSuccessStatusCode)
            {
                PartitionKey = partitionKey,
                RowKey = rowKey
            };

            await tableClient.AddEntityAsync(tableEntity);
            _logger.LogInformation("Data successfully saved to table storage.");
        }
    }
}
