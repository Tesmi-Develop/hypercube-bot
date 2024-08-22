using JetBrains.Annotations;

namespace HypercubeBot.Environments;

[PublicAPI]
public sealed class EnvironmentData
{
    [EnvironmentComment("Uri to connect to MongoDB")]
    public readonly string MongoUri = "mongodb://localhost:27017/";
    [EnvironmentComment("Name of the MongoDB database")]
    public readonly string MongoDatabaseName = "main";
    
    [EnvironmentSpacing]
    
    public readonly string DiscordApiUri = "https://discord.com/";
    public readonly string DiscordApiRoute = "api/v10";
    public readonly string DiscordApiOauthRoute = "oauth2/authorize";
    
    [EnvironmentSpacing]
    
    [EnvironmentComment("Bot token")]
    public readonly string DiscordToken = default!;
    [EnvironmentComment("Bot id")]
    public readonly string DiscordClientId = default!;
    [EnvironmentComment("Bot secret")]
    public readonly string DiscordClientSecret = default!;
    [EnvironmentComment("Guild id for bot testing")]
    public readonly string DiscordDevelopmentGuildId = default!;
    
    [EnvironmentSpacing]
    
    [EnvironmentComment("oauth server address")]
    public readonly string DiscordOauthHost = "http://localhost:2424/";
    [EnvironmentComment("oauth server redirect address")]
    public readonly string DiscordOauthRedirectHost = "http://localhost:2424/";
    [EnvironmentComment("oauth server redirect route")]
    public readonly string DiscordOauthRedirectRoute = "auth/callback";
    
    [EnvironmentSpacing]
    [EnvironmentComment("Time in seconds between each update that updates user roles")]
    public readonly string TrackingRate = "10";
}