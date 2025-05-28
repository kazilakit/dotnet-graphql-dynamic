using System;
using GraphQL.DomainService.Entities;
using GraphQL.DomainService.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace GraphQL.DomainService.Repositories;

public class SchemaDefinitionRepository : IRepository<SchemaDefinition>
{
    private readonly IMongoDatabase _database;

    public SchemaDefinitionRepository(IMongoDatabase database)
    {
        _database = database;
    }

    public async Task<List<SchemaDefinition>> GetItemsAsync(string collectionName,
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

        var list = await query
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();
        return list.Select(doc => BsonSerializer.Deserialize<SchemaDefinition>(doc)).ToList();
    }

    public async Task<(List<SchemaDefinition> items, long count)> GetItemsWithCountAsync(string collectionName,
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
        var list = await query
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();

        var items = list.Select(doc => BsonSerializer.Deserialize<SchemaDefinition>(doc)).ToList();
        return (items, count);
    }
    public async Task<SchemaDefinition> InsertAsync(string collectionName, SchemaDefinition data)
    {
        var collection = _database.GetCollection<SchemaDefinition>(collectionName);
        await collection.InsertOneAsync(data);
        return data;
    }

    public async Task<ActionResponse> UpdateAsync(string collectionName,
        FilterDefinition<SchemaDefinition> filter,
        SchemaDefinition data)
    {
        var collection = _database.GetCollection<SchemaDefinition>(collectionName);
        //var update = Builders<SchemaDefinition>.Update.Set(x => x, data);
        var result = await collection.ReplaceOneAsync(filter, data);//UpdateOneAsync(filter, update);
        var actionResponse = new ActionResponse
        {
            Acknowledged = result.IsAcknowledged,
            TotalImpactedData = result.ModifiedCount
        };
        return actionResponse;
    }

    public async Task<ActionResponse> DeleteAsync(string collectionName, FilterDefinition<SchemaDefinition> filter)
    {
        var collection = _database.GetCollection<SchemaDefinition>(collectionName);
        var result = await collection.DeleteOneAsync(filter);
        var actionResponse = new ActionResponse
        {
            Acknowledged = result.IsAcknowledged,
            TotalImpactedData = result.DeletedCount
        };
        return actionResponse;
    }

    public async Task<SchemaDefinition?> GetItemAsync(string collectionName, string id)
    {
        var collection = _database.GetCollection<SchemaDefinition>(collectionName);
        var filter = Builders<SchemaDefinition>.Filter.Eq(x => x.ItemId, id);
        return await collection.Find(filter).FirstOrDefaultAsync();
    }
}
