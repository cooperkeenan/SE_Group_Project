using Microsoft.Extensions.Configuration;
using System.Collections.Generic;


namespace EnviroMonitorApp.Services
{
    public class ApiKeyProvider
    {
        private readonly IConfiguration _configuration;

        // Constructor where the IConfiguration is injected
        public ApiKeyProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Example key retrieval using IConfiguration
        public string OpenAqKey => _configuration["ApiKeys:OpenAq"];
        public string OpenWeatherMap => _configuration["ApiKeys:OpenWeatherMap"];

        // Indexer to access API keys
        public string this[string key] => _configuration[$"ApiKeys:{key}"];
    }
}

