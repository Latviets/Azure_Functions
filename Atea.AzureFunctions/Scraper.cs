using System;
using System.Collections.Concurrent;
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

        public Scraper(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Scraper>();
        }

        [Function("Scraper")]
        public async Task Run([TimerTrigger("* * * * *")] TimerInfo myTimer)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = _baseAdress;
                    var response = await client.GetAsync("/v3.1/lang/spanish");
                    var result = await response.Content.ReadFromJsonAsync<ICollection<Country>>();

                    var partitionKey = Guid.NewGuid().ToString();
                    var rowKey = Guid.NewGuid().ToString();

                    var tableServiceClient = new TableServiceClient(_connectionString);
                    await tableServiceClient.CreateTableIfNotExistsAsync("atea");
                    var tableClient = tableServiceClient.GetTableClient("atea");
                    await tableClient.AddEntityAsync(new TableEntity(response.IsSuccessStatusCode)
                    {
                        PartitionKey = partitionKey,
                        RowKey = rowKey
                    });

                    var newBlobContainerName = $"testingblob-{Guid.NewGuid().ToString().Replace("-", string.Empty).ToLower()}";

                    var blobServiceClient = new BlobServiceClient(_connectionString);
                    var blobContainerClient = blobServiceClient.GetBlobContainerClient(newBlobContainerName);
              
                    await blobContainerClient.CreateIfNotExistsAsync();
                    await blobContainerClient.UploadBlobAsync($"TestingBlob_{rowKey}.json", BinaryData.FromObjectAsJson(result));

                    var blob = blobContainerClient.GetBlobClient($"TestingBlob_{rowKey}.json");
                    var content = await blob.DownloadContentAsync();

                    content.Value.Content.ToObjectFromJson<Country>();

                    _logger.LogInformation("Request was processed.");
                }
            }
            catch (Exception ex)
            {
                Console.Write("Message :{0}", ex.Message);
            }
        }              

    }
}
