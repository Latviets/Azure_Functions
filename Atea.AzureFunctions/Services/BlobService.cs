using Atea.AzureFunctions.Models;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;

namespace Atea.AzureFunctions.Services
{
    public class BlobService
    {
        private readonly string _connectionString;
        private readonly ILogger _logger;
        private readonly string _uniqueBlobContainerName = $"testingblob-{Guid.NewGuid().ToString().Replace("-", string.Empty).ToLower()}";

        public BlobService(string connectionString, ILogger logger)
        {
            _connectionString = connectionString;
            _logger = logger;
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
