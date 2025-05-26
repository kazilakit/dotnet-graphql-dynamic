using System;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GraphQL.DomainService.Repositories;

public interface IRepository<T>
{
    Task<List<T>> GetItemsAsync(string collectionName,
        FilterDefinition<BsonDocument> filter,
        SortDefinition<BsonDocument>? sort = null,
        ProjectionDefinition<BsonDocument>? projection = null,
        int skip = 0,
        int limit = 10);

    Task<(List<T> items, long count)> GetItemsWithCountAsync(string collectionName,
        FilterDefinition<BsonDocument> filter,
        SortDefinition<BsonDocument>? sort = null,
        ProjectionDefinition<BsonDocument>? projection = null,
        int skip = 0,
        int limit = 10);

    Task<T?> GetItemAsync(string collectionName, string id);
    Task<T> InsertAsync(string collectionName, T data);
    Task<UpdateResult> UpdateAsync(string collectionName, FilterDefinition<T> filter, T data);
    Task<DeleteResult> DeleteAsync(string collectionName, FilterDefinition<T> filter);
}
