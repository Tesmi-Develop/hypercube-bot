using System.Net;
using System.Net.Http.Headers;
using Discord;
using Discord.Rest;
using Hypercube.Dependencies;
using Hypercube.Shared.Logging;
using HypercubeBot.Data;
using HypercubeBot.Schemas;
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
    public string ListenUrl => Environment.GetEnvironmentVariable("LISTEN_URI") ?? "http://localhost:8080/";

    [Dependency] private readonly MongoService _mongoService = default!;
   
    private readonly Logger _logger = default!;
    private HttpListener _listener = default!;
    private HttpClient _client = default!;
    
    public async Task Start()
    {
        _client = new HttpClient();
        _listener = new HttpListener();
        _listener.Prefixes.Add(ListenUrl);
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
           
           await ProcessApiToken(apiToken);
        }
    }

    private async Task ProcessApiToken(DiscordApiToken apiToken)
    {
        await using var client = new DiscordRestClient();
        await client.LoginAsync(TokenType.Bearer, apiToken.AccessToken);

        var userId = client.CurrentUser.Id.ToString();
        var user = _mongoService.GetData<DiscordUserSchema>(userId);
        
        user.Mutate(draft =>
        {
            draft.AccessToken = apiToken.AccessToken;
            draft.RefreshToken = apiToken.RefreshToken;
            draft.TokenExpiration = DateTime.Now.AddSeconds(Convert.ToDouble(apiToken.ExpiresIn));
        });
        
        _logger.Debug($"Logged new user {client.CurrentUser.Username}");
    }

    public async Task RefreshToken(string userId)
    {
        if (!_mongoService.HaveData<DiscordUserSchema>(userId))
        {
            throw new ArgumentException("User not found");
        }

        await RefreshToken(_mongoService.GetData<DiscordUserSchema>(userId));
    }
    
    public async Task RefreshToken(DataWrapper<DiscordUserSchema> userData)
    {
        var data = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", userData.Data.RefreshToken)
        });

        var response = await _client.PostAsync(DiscordUrl, data);
        var content = await response.Content.ReadAsStringAsync();
        
        var apiToken = JsonSerializer.Deserialize<DiscordApiToken>(content);
        if (apiToken is null) return;
        
        userData.Mutate(draft =>
        {
            draft.AccessToken = apiToken.AccessToken;
            draft.RefreshToken = apiToken.RefreshToken;
            draft.TokenExpiration = DateTime.Now.AddSeconds(Convert.ToDouble(apiToken.ExpiresIn));
        });
        
        _logger.Debug($"Token refreshed for user {userData.Data.Id}");
    }
}