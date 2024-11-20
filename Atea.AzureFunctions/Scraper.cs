using System.Net.Http.Json;
using Atea.AzureFunctions.Models;
using Atea.AzureFunctions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Atea.AzureFunctions
{
    public class Scraper
    {
        private readonly ILogger _logger;
        private readonly DataService _dataService;
        private readonly StorageService _storageService;
        private readonly BlobService _blobService;

        private Uri _baseAdress = new Uri("https://restcountries.com");
        private string _tableName = "atea";
        private readonly string _connectionString = "UseDevelopmentStorage=true";

        public Scraper(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Scraper>();
            _dataService = new DataService(_baseAdress, _logger);
            _storageService = new StorageService(_connectionString, _logger, _tableName);
            _blobService = new BlobService(_connectionString, _logger);
        }

        [Function("Scraper")]
        public async Task Run([TimerTrigger("* * * * *")] TimerInfo myTimer)
        {
            try
            {
                var response = await _dataService.GetDataAsync();

                if (response != null && response.IsSuccessStatusCode)
                {
                    var countries = await response.Content.ReadFromJsonAsync<ICollection<Country>>();

                    if (countries != null)
                    {
                        var partitionKey = GenerateUniquePartitionKey();
                        var rowKey = GenerateUniqueRowKey();

                        await _storageService.SaveDataToTableStorageAsync(partitionKey, rowKey, response);
                        await _blobService.SaveToBlobStorageAsync(rowKey, countries);

                        var downloadedCountries = await _blobService.DownloadBlobContentsAsync(rowKey);
                    }

                    _logger.LogInformation("Request was processed.");
                }
            }
            catch (Exception ex)
            {
                Console.Write("Message :{0}", ex.Message);
            }
        }

        private string GenerateUniquePartitionKey()
        {
            return Guid.NewGuid().ToString();
        }

        private string GenerateUniqueRowKey()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
