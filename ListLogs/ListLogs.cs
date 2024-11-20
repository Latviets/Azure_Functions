using ListLogs.Services;
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
        private string _table = "atea";
        private readonly LogsService _logsService;

        public ListLogs(ILogger<ListLogs> logger)
        {
            _logger = logger;
            _logsService = new LogsService(_connectionString, _table);
        }

        [Function("ListLogs")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            var fromResult = ValidateDateQuery(req, "from", out var from);
            if (fromResult != null) return fromResult;

            var toResult = ValidateDateQuery(req, "to", out var to);
            if (toResult != null) return toResult;

            var logs = _logsService.GetLogsInGivenTimeRange(from, to);
            _logger.LogInformation($"Found {logs.Result.Count} logs from {from} to {to}.");

            return new OkObjectResult(logs);
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
