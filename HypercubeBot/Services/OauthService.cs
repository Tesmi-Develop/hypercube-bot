﻿using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Discord;
using Discord.Rest;
using Hypercube.Dependencies;
using Hypercube.Shared.Logging;
using HypercubeBot.Data;
using HypercubeBot.Environments;
using HypercubeBot.Schemas;
using HypercubeBot.ServiceRealisation;

namespace HypercubeBot.Services;

[Service]
public sealed class OauthService : IStartable
{
    [Dependency] private readonly EnvironmentData _environmentData = default!;
    [Dependency] private readonly MongoService _mongoService = default!;
   
    private readonly Logger _logger = default!;
    private readonly HttpListener _listener = new();
    private readonly HttpClient _client = new();
    
    private static string SanitizePath(string path)
    {
        if (path.Last() == '/')
            path = path[..^1];
        return path;
    }
    
    public async Task Start()
    {
        _listener.Prefixes.Add(_environmentData.DiscordOauthHost);
        _listener.Start();
        _logger.Debug($"Started listener on {_environmentData.DiscordOauthHost}");
        
        var clientId = _environmentData.DiscordClientId;
        var clientSecret = _environmentData.DiscordClientSecret;
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

    private async Task ProcessContextAsync(HttpListenerContext context)
    {
        var path = SanitizePath(context.Request.Url?.AbsolutePath ?? "");
        
        if (path != $"/{_environmentData.DiscordOauthRedirectRoute}")
            return;
        
        var code = context.Request.QueryString["code"];
        if (code is null) 
            return;
          
        var data = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("redirect_uri", $"{_environmentData.DiscordOauthRedirectHost}{_environmentData.DiscordOauthRedirectRoute}"),
        });

        var response = await _client.PostAsync($"{_environmentData.DiscordApiUri}{_environmentData.DiscordApiRoute}/oauth2/token", data);
        var content = await response.Content.ReadAsStringAsync();
        
        var apiToken = JsonSerializer.Deserialize<DiscordApiToken>(content);
        if (apiToken is null) 
            return;
           
        await ProcessApiToken(apiToken);
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
    
    public async Task RefreshToken(DataWrapper<DiscordUserSchema> userData)
    {
        var data = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", userData.Data.RefreshToken)
        });

        var response = await _client.PostAsync($"{_environmentData.DiscordApiRoute}/oauth2/token", data);
        var content = await response.Content.ReadAsStringAsync();
        
        var apiToken = JsonSerializer.Deserialize<DiscordApiToken>(content);
        if (apiToken is null) 
            return;
        
        userData.Mutate(draft =>
        {
            draft.AccessToken = apiToken.AccessToken;
            draft.RefreshToken = apiToken.RefreshToken;
            draft.TokenExpiration = DateTime.Now.AddSeconds(Convert.ToDouble(apiToken.ExpiresIn));
        });
        
        _logger.Debug($"Token refreshed for user {userData.Data.Id}");
    }
}