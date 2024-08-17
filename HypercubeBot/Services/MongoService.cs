using System.Diagnostics;
using Hypercube.Shared.Logging;
using HypercubeBot.Schemas;
using HypercubeBot.ServiceRealisation;
using MongoDB.Driver;

namespace HypercubeBot.Services;

[Service]
public class MongoService : IInitializable
{
    private MongoClient _client = default!;
    private IMongoDatabase _database = default!;
    private IMongoCollection<Guild> _collection = default!;
    private readonly Logger _logger = default!;
    
    public void Init()
    {
        var mongoUri = Environment.GetEnvironmentVariable("MONGO_URI");
        var databaseName = Environment.GetEnvironmentVariable("MONGO_DATABASE_NAME");
        
        Debug.Assert(mongoUri != null, "You must set your 'MONGODB_URI' environment variable.");
        Debug.Assert(databaseName != null, "You must set your 'MONGO_DATABASE_NAME' environment variable.");

        _client = new MongoClient(mongoUri);
        _logger.Debug("Mongo client created");
        
        _database = _client.GetDatabase(databaseName);
        _collection = _database.GetCollection<Guild>("Guilds");
    }

    public GuildWrapper GetGuild(string guildId)
    {
        return new GuildWrapper(guildId, _collection);
    }
}