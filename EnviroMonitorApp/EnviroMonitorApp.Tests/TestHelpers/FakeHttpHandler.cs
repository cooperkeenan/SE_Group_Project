using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EnviroMonitorApp.Tests.TestHelpers;

public sealed class FakeHttpHandler : HttpMessageHandler
{
    private readonly string _json;
    private readonly HttpStatusCode _code;

    public FakeHttpHandler(string json, HttpStatusCode code)
        => (_json, _code) = (json, code);

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        => Task.FromResult(new HttpResponseMessage(_code)
        {
            Content = new StringContent(_json, Encoding.UTF8, "application/json")
        });
}
