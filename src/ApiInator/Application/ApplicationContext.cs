using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using ApiInator.Model;

public class ApplicationContext
{
    private readonly IMongoDatabase _database;

    public ApplicationContext(string connectionString)
    {
        var mongoUrl = new MongoUrl(connectionString);
        var client = new MongoClient(mongoUrl);
        _database = client.GetDatabase(mongoUrl.DatabaseName ?? "WorthinatorDb");

        ConfigureMappings();
    }

    public IMongoCollection<GameData> Games => _database.GetCollection<GameData>("Games");

    private void ConfigureMappings()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(GameData)))
        {
            BsonClassMap.RegisterClassMap<GameData>(cm =>
            {
                cm.AutoMap();
                cm.MapIdProperty(g => g.Id)
                    .SetIdGenerator(StringObjectIdGenerator.Instance)
                    .SetSerializer(new StringSerializer(BsonType.ObjectId))
                    .SetIgnoreIfDefault(true);
            });
        }
    }
}