using MongoDB.Driver;
using Shared.Services.Abstractions;

namespace Shared.Services;

public class MongoDbService : IMongoDbService
{
    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        IMongoDatabase database = GetDatabase();

        return database.GetCollection<T>(collectionName);
    }

    public IMongoDatabase GetDatabase(string databaseName = "ProductDb",
        string connectionString = "mongodb://localhost:27017")
    {
        MongoClient client = new(connectionString);

        return client.GetDatabase(databaseName);
    }
}