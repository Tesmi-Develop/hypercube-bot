using System.Net;
using System.Net.Http.Headers;
using HypercubeBot.Data;
using HypercubeBot.ServiceRealisation;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace HypercubeBot.Services;

[Service]
public class OauthService : IStartable
{
    public string RedirectUrl => $"{Environment.GetEnvironmentVariable("REDIRECT_URL")}";
    public string DiscordUrl => $"{Environment.GetEnvironmentVariable("DISCORD_API_ENDPOINT")}/oauth2/token";
    public string HTTPUrl => Environment.GetEnvironmentVariable("HTTP_URI") ?? "http://localhost:8080/";
    
    private HttpListener _listener;
    private HttpClient _client;
    
    public async Task Start()
    {
        _client = new HttpClient();
        _listener = new HttpListener();
        _listener.Prefixes.Add(HTTPUrl);
        _listener.Start();
        
        var clientId = Environment.GetEnvironmentVariable("CLIENT_ID") ?? "0";
        var clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET") ?? "0";
        var authValue = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
        
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
        
        while (_listener.IsListening)
        {
            var context = await _listener.GetContextAsync();
            await ProcessContextAsync(context);
            
            var response = context.Response;
            response.Close();
        }
    }
    
    private string SanitizePath(string path)
    {
        if (path.Last() == '/')
            path = path[..^1];
        return path;
    }

    private async Task ProcessContextAsync(HttpListenerContext context)
    {
        var path = SanitizePath(context.Request.Url?.AbsolutePath ?? "");
        
        if (path == $"/{RedirectUrl}")
        {
           var code = context.Request.QueryString["code"];
           if (code is null) return;

          
           var data = new FormUrlEncodedContent(new[]
           {
               new KeyValuePair<string, string>("grant_type", "authorization_code"),
               new KeyValuePair<string, string>("code", code),
               new KeyValuePair<string, string>("redirect_uri", $"{HTTPUrl}{RedirectUrl}")
           });

           var response = await _client.PostAsync(DiscordUrl, data);
           var content = await response.Content.ReadAsStringAsync();
           
           var apiToken = JsonSerializer.Deserialize<DiscordApiToken>(content);
           if (apiToken is null) return;
           
           Console.WriteLine(apiToken!.AccessToken);
        }
    }
}