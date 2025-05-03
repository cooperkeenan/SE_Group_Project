using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EnviroMonitorApp.Services
{
    /// <summary>
    /// Dumps every request/response to Debug so you can
    /// watch JSON (or errors) in logcat / VS output.
    /// </summary>
    public class HttpLoggingHandler : DelegatingHandler
    {
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
