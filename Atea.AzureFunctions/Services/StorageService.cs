using Atea.AzureFunctions.Models;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;


namespace Atea.AzureFunctions.Services
{
    public class StorageService
    {
        private readonly string _connectionString;
        private readonly ILogger _logger;
        private readonly string _uniqueBlobContainerName;
        private readonly string _storageTableName;

        public StorageService(string connectionString, ILogger logger, string storageTableName)
        {
            _connectionString = connectionString;
            _logger = logger;
            _uniqueBlobContainerName = $"testingblob-{Guid.NewGuid().ToString().Replace("-", string.Empty).ToLower()}";
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

        public async Task SaveToBlobStorageAsync(string rowKey, ICollection<Country> countries)
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(_uniqueBlobContainerName);

            await blobContainerClient.CreateIfNotExistsAsync();
            await blobContainerClient.UploadBlobAsync($"TestingBlob_{rowKey}.json", BinaryData.FromObjectAsJson(countries));

            _logger.LogInformation("Data saved to blob storage.");
        }

        public async Task<ICollection<Country>> DownloadBlobContentsAsync(string rowKey)
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(_uniqueBlobContainerName);
            var blobClient = blobContainerClient.GetBlobClient($"TestingBlob_{rowKey}.json");

            var content = await blobClient.DownloadContentAsync();
            return content.Value.Content.ToObjectFromJson<ICollection<Country>>();
        }
    }
}
