namespace EnviroMonitorApp.Services
{
    /// <summary>
    /// Provides API keys for external services such as OpenAQ and OpenWeatherMap.
    /// Contains default keys for testing and allows custom keys to be provided.
    /// </summary>
    public class ApiKeyProvider
    {
        // These are the real keys or default ones for testing
        private string _openAqKey = "sample-openaq-key";
        private string _openWeatherMapKey = "sample-openweathermap-key";

        /// <summary>
        /// Initializes a new instance of the ApiKeyProvider class.
        /// </summary>
        /// <param name="openAqKey">Custom OpenAQ API key (optional). If null or empty, the default key is used.</param>
        /// <param name="openWeatherMapKey">Custom OpenWeatherMap API key (optional). If null or empty, the default key is used.</param>
        public ApiKeyProvider(string openAqKey = null, string openWeatherMapKey = null)
        {
            if (!string.IsNullOrEmpty(openAqKey))
                _openAqKey = openAqKey;
            
            if (!string.IsNullOrEmpty(openWeatherMapKey))
                _openWeatherMapKey = openWeatherMapKey;
        }

        /// <summary>
        /// Gets the OpenAQ API key.
        /// </summary>
        public string OpenAqKey => _openAqKey;
        
        /// <summary>
        /// Gets the OpenWeatherMap API key.
        /// </summary>
        public string OpenWeatherMap => _openWeatherMapKey;
    }
}