using Microsoft.Extensions.Logging;

namespace Atea.AzureFunctions.Services
{
    public class DataService
    {
        private readonly Uri _baseAdress;
        private readonly ILogger _logger;
        public DataService(Uri baseAdress, ILogger logger) 
        { 
            _baseAdress = baseAdress;
            _logger = logger;
        }
        public async Task<HttpResponseMessage> GetDataAsync()
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
    }
}
