using System.Net;
using System.Text;
using HypercubeBot.ServiceRealisation;

namespace HypercubeBot.Services;

[Service]
public class HttpService
{
    private readonly HttpClient _client = new();

    public async Task<string> GetAsync(string uri)
    {
        var message = new HttpRequestMessage(HttpMethod.Get, uri);
        message.Headers.Add("user-agent", "C#");
        
        using var response = await _client.SendAsync(message);
        return await response.Content.ReadAsStringAsync();
    }
}