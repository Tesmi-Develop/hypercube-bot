using MongoDB.Driver;

namespace HypercubeBot.Schemas;

public class DataWrapper<T, TReadonly> where T : TReadonly where TReadonly : Schema
{
    public TReadonly Data => _data;

    private readonly T _data;
    private readonly IMongoCollection<T> _collection;
    private readonly string _id;
    
    public DataWrapper(IMongoCollection<T> collection, string id, Action<TReadonly>? onCreate = null)
    {
        _id = id;
        _collection = collection;
        _data = _collection.Find(item => item.Id == _id).FirstOrDefault();
        
        if (_data is not null) 
            return;
        
        _data = (T)Activator.CreateInstance(typeof(T), args: [_id])!;
        _collection.InsertOne(_data);
        onCreate?.Invoke(_data);
    }

    public void Mutate(Action<T> mutator)
    {
        mutator(_data);
        Update();
    }

    private void Update()
    {
        _collection.ReplaceOne(item => item.Id == _id, _data);
    }
}