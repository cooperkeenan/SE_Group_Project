using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EnviroMonitorApp.Services
{
    /// <summary>
    /// HTTP message handler that logs all requests and responses to the debug output.
    /// Useful for debugging API calls by showing full request/response details in the console.
    /// </summary>
    public class HttpLoggingHandler : DelegatingHandler
    {
        /// <summary>
        /// Intercepts HTTP requests, logs them, and then forwards them to the inner handler.
        /// After receiving a response, logs it and returns it to the caller.
        /// </summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <param name="ct">Cancellation token to cancel the operation.</param>
        /// <returns>The HTTP response message from the inner handler.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken ct)
        {
            Debug.WriteLine($"[HTTP] → {request.Method} {request.RequestUri}");

            if (request.Content != null)
                Debug.WriteLine($"[HTTP]   ReqBody: {await request.Content.ReadAsStringAsync()}");

            var response = await base.SendAsync(request, ct);

            Debug.WriteLine($"[HTTP] ← {(int)response.StatusCode} {response.ReasonPhrase}");
            Debug.WriteLine($"[HTTP]   RespBody: {await response.Content.ReadAsStringAsync()}");

            return response;
        }
    }
}