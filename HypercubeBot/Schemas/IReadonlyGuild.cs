using MongoDB.Bson;

namespace HypercubeBot.Schemas;

public interface IReadonlyGuild
{
    ObjectId DataId { get; }
    string GuildId { get; }
    Dictionary<string, string> Repositories { get; }
}