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
        private readonly string _connectionString = "UseDevelopmentStorage=true";
        private Uri _baseAdress = new Uri("https://restcountries.com");
        private string _table = "atea";
        public ListLogs(ILogger<ListLogs> logger)
        {
            _logger = logger;
        }

        [Function("ListLogs")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            var fromResult = ValidateDateQuery(req, "from", out var from);
            if (fromResult != null) return fromResult;

            var toResult = ValidateDateQuery(req, "to", out var to);
            if (toResult != null) return toResult;

            List<TableEntity> logs = await GetLogsInGivenTimeRange(from, to);

            _logger.LogInformation($"Found {logs.Count} logs from {from} to {to}.");

            return new OkObjectResult(logs);
        }

        private async Task<List<TableEntity>> GetLogsInGivenTimeRange(DateTimeOffset from, DateTimeOffset to)
        {
            var tableServiceClient = new TableServiceClient(_connectionString);
            await tableServiceClient.CreateTableIfNotExistsAsync(_table);
            var tableClient = tableServiceClient.GetTableClient(_table);

            var queryResults = tableClient.Query<TableEntity>(
                item => item.Timestamp.Value >= from && item.Timestamp.Value <= to);

            var logs = queryResults.ToList();
            return logs;
        }

        private IActionResult ValidateDateQuery(HttpRequest req, string queryParam, out DateTimeOffset date)
        {
            if (!DateTimeOffset.TryParse(req.Query[queryParam], out date))
            {
                return new BadRequestObjectResult($"Invalid '{queryParam}' date in request.");
            }

            return null;
        }
    }
}
