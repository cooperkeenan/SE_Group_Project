namespace EnviroMonitorApp.Services
{
    public class ApiKeyProvider
    {
        // These are the real keys or default ones for testing
        private string _openAqKey = "sample-openaq-key";
        private string _openWeatherMapKey = "sample-openweathermap-key";

        // Constructor that can be used for testing (can pass in keys directly)
        public ApiKeyProvider(string openAqKey = null, string openWeatherMapKey = null)
        {
            if (!string.IsNullOrEmpty(openAqKey))
                _openAqKey = openAqKey;
            
            if (!string.IsNullOrEmpty(openWeatherMapKey))
                _openWeatherMapKey = openWeatherMapKey;
        }

        public string OpenAqKey => _openAqKey;
        public string OpenWeatherMap => _openWeatherMapKey;
    }
}
