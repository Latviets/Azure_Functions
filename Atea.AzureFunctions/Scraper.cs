using System.Net.Http.Json;
using Atea.AzureFunctions.Models;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Atea.AzureFunctions
{
    public class Scraper
    {
        private readonly ILogger _logger;
        private readonly string _connectionString = "UseDevelopmentStorage=true";
        private Uri _baseAdress = new Uri("https://restcountries.com");
        private string _table = "atea";
        private readonly string _uniqueblobStorageName = $"testingblob-{Guid.NewGuid().ToString().ToLower()}";

        public Scraper(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Scraper>();
        }

        [Function("Scraper")]
        public async Task Run([TimerTrigger("* * * * *")] TimerInfo myTimer)
        {
            try
            {
                var response = await GetCountriesAsync();

                if (response != null && response.IsSuccessStatusCode)
                {
                    var countries = await response.Content.ReadFromJsonAsync<ICollection<Country>>();

                    if (countries != null)
                    {
                        var partitionKey = GenerateUniquePartitionKey();
                        var rowKey = GenerateUniqueRowKey();

                        await SaveDataToTableStorageAsync(partitionKey, rowKey, response);
                        await SaveToBlobStorageAsync(rowKey, countries);

                        var downloadedCountries = await DownloadBlobContentsAsync(rowKey);
                    }

                    _logger.LogInformation("Request was processed.");
                }
            }
            catch (Exception ex)
            {
                Console.Write("Message :{0}", ex.Message);
            }
        }

        private async Task<HttpResponseMessage> GetCountriesAsync()
        {
            using var client = new HttpClient { BaseAddress = _baseAdress };
            var response = await client.GetAsync("/v3.1/lang/spanish");

            if (response.IsSuccessStatusCode)
            {
                return response;
            }

            _logger.LogError("Failed to get response.");
            return null;
        }

        private string GenerateUniquePartitionKey()
        {
            return Guid.NewGuid().ToString();
        }

        private string GenerateUniqueRowKey()
        {
            return Guid.NewGuid().ToString();
        }

        private async Task SaveDataToTableStorageAsync(string partitionkey, string rowkey, HttpResponseMessage response)
        {
            var tableServiceClient = new TableServiceClient(_connectionString);
            await tableServiceClient.CreateTableIfNotExistsAsync(_table);
            var tableClient = tableServiceClient.GetTableClient(_table);

            var tableEntity = new TableEntity(response.IsSuccessStatusCode)
            {
                PartitionKey = partitionkey,
                RowKey = rowkey
            };

            await tableClient.AddEntityAsync(tableEntity);
            _logger.LogInformation("Data successfully saved to table storage.");
        }

        private async Task SaveToBlobStorageAsync(string rowkey, ICollection<Country> countries)
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(_uniqueblobStorageName);

            await blobContainerClient.CreateIfNotExistsAsync();
            await blobContainerClient.UploadBlobAsync($"TestingBlob_{rowkey}.json", BinaryData.FromObjectAsJson(countries));

            _logger.LogInformation("Data saved to blob storage.");
        }

        private async Task<ICollection<Country>> DownloadBlobContentsAsync(string rowKey)
        {
            var bloblServiceClient = new BlobServiceClient(_connectionString);
            var blobContainerClient = bloblServiceClient.GetBlobContainerClient(_uniqueblobStorageName);

            var blobClient = blobContainerClient.GetBlobClient($"TestingBlob_{rowKey}.json");
            var content = await blobClient.DownloadContentAsync();

            _logger.LogInformation("Data downloaded from blob storage.");

            return content.Value.Content.ToObjectFromJson<ICollection<Country>>();
        }
    }
}
