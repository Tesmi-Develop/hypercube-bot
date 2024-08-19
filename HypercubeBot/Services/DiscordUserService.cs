using Discord;
using Discord.Rest;
using Hypercube.Dependencies;
using Hypercube.Shared.Logging;
using HypercubeBot.Schemas;
using HypercubeBot.ServiceRealisation;

namespace HypercubeBot.Services;

[Service]
public sealed class DiscordUserService
{
    [Dependency] private readonly MongoService _mongoService = default!;
    [Dependency] private readonly OauthService _oauthService = default!;
    
    private readonly Logger _logger = default!;

    public async Task<DiscordRestClient?> TryGetUser(string userId)
    {
        try
        {
            return await GetUser(userId);
        }
        catch (Exception)
        {
            // ignored
        }

        return null;
    }
    
    public async Task<DiscordRestClient> GetUser(string userId)
    {
        if (!_mongoService.HaveData<DiscordUserSchema>(userId))
            throw new ArgumentException("User not found");

        var userData = _mongoService.GetData<DiscordUserSchema>(userId);
        if (userData.Data.TokenExpiration <= DateTime.Now)
        {
            _logger.Debug($"Token expired for user {userId}");
            await _oauthService.RefreshToken(userData);
        }

        var client = new DiscordRestClient();
        await client.LoginAsync(TokenType.Bearer, userData.Data.AccessToken);

        return client;
    }
}