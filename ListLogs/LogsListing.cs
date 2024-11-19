using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ListLogs
{
    public class LogsListing
    {
        private readonly ILogger<LogsListing> _logger;

        public LogsListing(ILogger<LogsListing> logger)
        {
            _logger = logger;
        }

        [Function("LogsListing")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            DateTimeOffset.TryParse(req.Query["from"], out var from);
            DateTimeOffset.TryParse(req.Query["to"], out var to);

            var tableServiceClient = new TableServiceClient("UseDevelopmentStorage=true");
            await tableServiceClient.CreateTableIfNotExistsAsync("atea");
            var tableClient = tableServiceClient.GetTableClient("atea");

            var records = tableClient.Query<TableEntity>(
                item => item.Timestamp.Value >= from && item.Timestamp.Value <= to);
            var response = records.AsPages(null, 50);

            _logger.LogInformation("C# HTTP trigger function processed a request.");

            return new OkObjectResult(response);
        }
    }
}
