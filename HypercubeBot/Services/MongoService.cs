using System.Collections.Frozen;
using System.Diagnostics;
using Hypercube.Shared.Logging;
using HypercubeBot.Schemas;
using HypercubeBot.ServiceRealisation;
using HypercubeBot.Utils;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HypercubeBot.Services;

[Service]
public sealed class MongoService : IInitializable
{
    private MongoClient _client = default!;
    private IMongoDatabase _database = default!;
    private FrozenDictionary<Type, object> _collections;
    private readonly Logger _logger = default!;
    private readonly Dictionary<string, object> _dataWrappers = new();
    
    public void Init()
    {
        var mongoUri = Environment.GetEnvironmentVariable("MONGO_URI");
        var databaseName = Environment.GetEnvironmentVariable("MONGO_DATABASE_NAME");
        
        Debug.Assert(mongoUri != null, "You must set your 'MONGODB_URI' environment variable.");
        Debug.Assert(databaseName != null, "You must set your 'MONGO_DATABASE_NAME' environment variable.");

        _client = new MongoClient(mongoUri);
        _logger.Debug("Mongo client created");
        
        _database = _client.GetDatabase(databaseName);

        var dict = new Dictionary<Type, object>();

        foreach (var (type, attribute) in ReflectionHelper.GetAllTypes<CollectionAttribute>())
        {
            var name = attribute.Name ?? type.Name;

            _logger.Debug($"Register collection {name}");
            var method = _database.GetType().GetMethod(nameof(_database.GetCollection));
            if (method is null)
                continue;
            
            var generic = method.MakeGenericMethod([type]);
            dict[type] = generic.Invoke(_database, [name, null])!;
        }
        
        _collections = dict.ToFrozenDictionary();
    }

    public IEnumerable<DataWrapper<T, T>> GetData<T>() where T : Schema
    {
        return from item in ((IMongoCollection<T>)_collections[typeof(T)]).Find(_ => true).ToList() 
            select GetData<T>(item.Id);
    }

    public DataWrapper<T, T> GetData<T>(string id) where T : Schema
    {
        if (_dataWrappers.TryGetValue(id, out var wrapper))
            return (DataWrapper<T, T>)wrapper;

        return new DataWrapper<T, T>((IMongoCollection<T>)_collections[typeof(T)], id);
    }
}