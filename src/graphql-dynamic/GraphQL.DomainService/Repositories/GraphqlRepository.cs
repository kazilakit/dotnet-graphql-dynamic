using System;
using GraphQL.DomainService.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GraphQL.DomainService.Repositories;

public class GraphqlRepository : IRepository<BsonDocument>
{
    private readonly IMongoDatabase _database;

    public GraphqlRepository(IMongoDatabase database)
    {
        _database = database;
    }

    public async Task<List<BsonDocument>> GetItemsAsync(string collectionName,
        FilterDefinition<BsonDocument> filter,
        SortDefinition<BsonDocument>? sort = null,
        ProjectionDefinition<BsonDocument>? projection = null,
        int skip = 0,
        int limit = 10)
    {
        var collection = _database.GetCollection<BsonDocument>(collectionName);
        var query = collection.Find(filter);

        if (projection != null)
            query = query.Project(projection);

        if (sort != null)
            query = query.Sort(sort);

        return await query
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();
    }
    public async Task<(List<BsonDocument> items, long count)> GetItemsWithCountAsync(string collectionName,
        FilterDefinition<BsonDocument> filter,
        SortDefinition<BsonDocument>? sort = null,
        ProjectionDefinition<BsonDocument>? projection = null,
        int skip = 0,
        int limit = 10)
    {
        var collection = _database.GetCollection<BsonDocument>(collectionName);
        var query = collection.Find(filter);

        if (projection != null)
            query = query.Project(projection);

        if (sort != null)
            query = query.Sort(sort);

        var count = await query.CountDocumentsAsync();
        var items = await query
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();

        return (items, count);
    }
    public async Task<BsonDocument> InsertAsync(string collectionName, BsonDocument data)
    {
        var collection = _database.GetCollection<BsonDocument>(collectionName);
        await collection.InsertOneAsync(data);
        return data;
    }

    public async Task<ActionResponse> UpdateAsync(string collectionName,
        FilterDefinition<BsonDocument> filter,
        BsonDocument data)
    {
        var collection = _database.GetCollection<BsonDocument>(collectionName);
        var update = new BsonDocument("$set", data);
        var result = await collection.UpdateOneAsync(filter, update);
        var actionResponse = new ActionResponse
        {
            Acknowledged = result.IsAcknowledged,
            TotalImpactedData = result.ModifiedCount
        };
        return actionResponse;
    }

    public async Task<ActionResponse> DeleteAsync(string collectionName, FilterDefinition<BsonDocument> filter)
    {
        var collection = _database.GetCollection<BsonDocument>(collectionName);
        var result = await collection.DeleteOneAsync(filter);
        var actionResponse = new ActionResponse
        {
            Acknowledged = result.IsAcknowledged,
            TotalImpactedData = result.DeletedCount
        };
        return actionResponse;
    }

    public async Task<BsonDocument?> GetItemAsync(string collectionName, string id)
    {
        var collection = _database.GetCollection<BsonDocument>(collectionName);
        var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
        return await collection.Find(filter).FirstOrDefaultAsync();
    }
}
