using JetBrains.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HypercubeBot.Schemas;

[Serializable, Collection("Users"), PublicAPI]
public sealed class DiscordUser
{
    [BsonId]
    public ObjectId DataId { get; private set; }
    
    public string UserId { get; private set; }
    public string AccessToken { get; private set; }
    public string RefreshToken { get; private set; }
    public TimeSpan LastUpdate { get; private set; }
}