using System.Diagnostics;
using MongoDB.Driver;

namespace HypercubeBot.Schemas;

public class GuildWrapper
{
    public IReadonlyGuild Guild => _guild;
    public bool IsNewData => _isNewData;
    
    private bool _isNewData = false;
    private readonly Guild _guild;
    private readonly IMongoCollection<Guild> _collection;
    
    public GuildWrapper(string guildId, IMongoCollection<Guild> collection)
    {
        _collection = collection;
        _guild = _collection.Find(guild => guild.GuildId == guildId).FirstOrDefault();

        if (_guild is null)
        {
            _guild = new Guild(guildId);
            _isNewData = true;
        }
        
        if (!Has())
        {
            _collection.InsertOne(_guild);
        }
    }
    
    public GuildWrapper(Guild guildData, IMongoCollection<Guild> collection)
    {
        _collection = collection;
        _guild = guildData;
        
        if (!Has())
        {
            _collection.InsertOne(_guild);
        }
    }

    public void Mutate(Action<Guild> mutator)
    {
        mutator(_guild);
        Update();
    }

    public void Update()
    {
        _collection.ReplaceOne(guild => guild.GuildId == _guild.GuildId, _guild);
    }

    private bool Has()
    {
        return _collection.Find(guild => guild.GuildId == _guild.GuildId).FirstOrDefault() != null;
    }
}