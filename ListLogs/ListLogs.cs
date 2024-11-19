using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ListLogs
{
    public class ListLogs
    {
        private readonly ILogger<ListLogs> _logger;

        public ListLogs(ILogger<ListLogs> logger)
        {
            _logger = logger;
        }

        [Function("ListLogs")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            if (!DateTimeOffset.TryParse(req.Query["from"], out var from))
            {
                return new BadRequestObjectResult("Invalid 'from' date in request.");
            }

            if (!DateTimeOffset.TryParse(req.Query["to"], out var to))
            {
                return new BadRequestObjectResult("Invalid 'to' date in request.");
            }

            var tableServiceClient = new TableServiceClient("UseDevelopmentStorage=true");
            await tableServiceClient.CreateTableIfNotExistsAsync("atea");
            var tableClient = tableServiceClient.GetTableClient("atea");

            var queryResults = tableClient.Query<TableEntity>(
                item => item.Timestamp.Value >= from && item.Timestamp.Value <= to);

            var logs = queryResults.ToList();

            _logger.LogInformation($"Found {logs.Count} logs from {from} to {to}.");

            return new OkObjectResult(logs);
        }
    }
}
