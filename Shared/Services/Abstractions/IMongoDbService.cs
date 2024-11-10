using MongoDB.Driver;

namespace Shared.Services.Abstractions;

public interface IMongoDbService
{
    IMongoCollection<T> GetCollection<T>(string collectionName);
    IMongoDatabase GetDatabase(string databaseName, string connectionString);
}