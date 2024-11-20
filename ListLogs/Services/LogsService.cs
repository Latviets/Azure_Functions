using Azure.Data.Tables;

namespace ListLogs.Services
{
    public class LogsService
    {
        private readonly string _connectionString;
        private readonly string _tableName;

        public LogsService(string connection, string tableName) 
        { 
            
            _connectionString = connection;
            _tableName = tableName;

        }

        public async Task<List<TableEntity>> GetLogsInGivenTimeRange(DateTimeOffset from, DateTimeOffset to)
        {          
            var tableServiceClient = new TableServiceClient(_connectionString);
            await tableServiceClient.CreateTableIfNotExistsAsync(_tableName);
            var tableClient = tableServiceClient.GetTableClient(_tableName);

            var queryResults = tableClient.Query<TableEntity>(
                item => item.Timestamp.Value >= from && item.Timestamp.Value <= to);

            var logs = queryResults.ToList();
            return logs;
        }
    }
}
