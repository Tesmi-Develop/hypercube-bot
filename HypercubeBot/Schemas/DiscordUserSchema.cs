using JetBrains.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HypercubeBot.Schemas;

[Serializable, Collection("Users"), PublicAPI]
public sealed class DiscordUserSchema : Schema
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime TokenExpiration { get; set; }

    public DiscordUserSchema(string id) : base(id)
    {
    }
}