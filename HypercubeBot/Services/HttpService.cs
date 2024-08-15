using System.Net;
using System.Net.Http.Headers;
using System.Text;
using HypercubeBot.ServiceRealisation;

namespace HypercubeBot.Services;

[Service]
public class HttpService
{
    private readonly HttpClient _client;

    public HttpService()
    {
        HttpClientHandler handler = new HttpClientHandler 
        { 
            AutomaticDecompression = DecompressionMethods.All 
        };
        
        _client = new HttpClient();
    }

    public async Task<string> GetAsync(string uri)
    {
        var message = new HttpRequestMessage(HttpMethod.Get, uri);
        message.Headers.Add("user-agent", "C#");
        
        using var response = await _client.SendAsync(message);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> PostAsync(string uri, string data, string contentType)
    {
        using HttpContent content = new StringContent(data, Encoding.UTF8, contentType);
        
        HttpRequestMessage requestMessage = new HttpRequestMessage() 
        { 
            Content = content,
            Method = HttpMethod.Post,
            RequestUri = new Uri(uri)
        };
        
        using HttpResponseMessage response = await _client.SendAsync(requestMessage);

        return await response.Content.ReadAsStringAsync();
    }
}