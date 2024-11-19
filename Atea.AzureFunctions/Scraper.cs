using System;
using System.Net.Http.Json;
using Atea.AzureFunctions.Models;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;

namespace Atea.AzureFunctions
{
    public class Scraper
    {
        private readonly ILogger _logger;

        public Scraper(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Scraper>();
        }

        [Function("Scraper")]
        public async Task Run([TimerTrigger("*/1 * * * *")] TimerInfo myTimer)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://restcountries.com");
                    var response = await client.GetAsync("/v3.1/lang/spanish");
                    var result = await response.Content.ReadFromJsonAsync<ICollection<Country>>();
                    var key = Guid.NewGuid();

                    var tableServiceClient = new TableServiceClient("UseDevelopmentStorage=true");
                    await tableServiceClient.CreateTableIfNotExistsAsync("atea");
                    var tableClient = tableServiceClient.GetTableClient("atea");


                    await tableClient.AddEntityAsync(new TableEntity(response.IsSuccessStatusCode)
                    {
                        PartitionKey = key.ToString(),
                        RowKey = key.ToString()
                    });

                    var blobServiceClient = new BlobServiceClient("UseDevelopmentStorage=true");
                    var blobContainerClient = await blobServiceClient.CreateBlobContainerAsync("atea");

                    await blobContainerClient.Value.CreateIfNotExistsAsync();
                    await blobContainerClient.Value.UploadBlobAsync($"{key.ToString()}.json", BinaryData.FromObjectAsJson(result));

                    var blob = blobContainerClient.Value.GetBlobClient("container/table name");
                    var content = await blob.DownloadContentAsync();

                    content.Value.Content.ToObjectFromJson<Country>();

                    _logger.LogInformation("action");
                }
            }
            catch (Exception ex)
            {
                Console.Write("Message :{0}", ex.Message);
            }

        }
    }
}
