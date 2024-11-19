using System.Net.Http.Json;
using Atea.AzureFunctions.Models;
using Atea.AzureFunctions.Services;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Atea.AzureFunctions
{
    public class Scraper
    {
        private readonly ILogger _logger;
        private readonly StorageService _storageService;
        private Uri _baseAdress = new Uri("https://restcountries.com");
        private string _tableName = "atea";

        public Scraper(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Scraper>();
            _storageService = new StorageService("UseDevelopmentStorage=true", _logger, _tableName);
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

                        await _storageService.SaveDataToTableStorageAsync(partitionKey, rowKey, response);
                        await _storageService.SaveToBlobStorageAsync(rowKey, countries);

                        var downloadedCountries = await _storageService.DownloadBlobContentsAsync(rowKey);
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
    }
}
