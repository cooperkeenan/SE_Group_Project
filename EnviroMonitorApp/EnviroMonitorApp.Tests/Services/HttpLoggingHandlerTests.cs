using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EnviroMonitorApp.Services;
using EnviroMonitorApp.Tests.TestHelpers;
using Xunit;

namespace EnviroMonitorApp.Tests.Services
{
    public class HttpLoggingHandlerTests
    {
        [Fact]
        public async Task SendAsync_LogsRequestAndResponse()
        {
            // Arrange
            var jsonResponse = "{\"data\": \"test\"}";
            var innerHandler = new FakeHttpHandler(jsonResponse, HttpStatusCode.OK);
            var loggingHandler = new HttpLoggingHandler
            {
                InnerHandler = innerHandler
            };
            
            var client = new HttpClient(loggingHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api");
            request.Content = new StringContent("{\"query\": \"test\"}", Encoding.UTF8, "application/json");
            
            // Act
            var response = await client.SendAsync(request);
            
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal(jsonResponse, content);
            
            // Note: We can't directly test Debug.WriteLine output without more complex setup
            // This test primarily verifies that the handler doesn't throw exceptions
        }
        
        [Fact]
        public async Task SendAsync_HandlesRequestWithoutContent()
        {
            // Arrange
            var jsonResponse = "{\"data\": \"test\"}";
            var innerHandler = new FakeHttpHandler(jsonResponse, HttpStatusCode.OK);
            var loggingHandler = new HttpLoggingHandler
            {
                InnerHandler = innerHandler
            };
            
            var client = new HttpClient(loggingHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api");
            
            // Act
            var response = await client.SendAsync(request);
            
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal(jsonResponse, content);
        }
    }
}