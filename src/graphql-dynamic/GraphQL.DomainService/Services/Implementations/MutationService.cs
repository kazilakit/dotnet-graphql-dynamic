using GraphQL.DomainService.Entities;
using GraphQL.DomainService.Helpers;
using GraphQL.DomainService.Models;
using GraphQL.DomainService.Models.Constants;
using GraphQL.DomainService.Repositories;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using MongoDB.Bson;

namespace GraphQL.DomainService.Services;

public class MutationService : IMutationService
{
    private readonly IRepository<BsonDocument> _repository;
    public MutationService(IRepository<BsonDocument> repository)
    {
        _repository = repository;
    }
    public async Task<ActionResponse> InsertAsync(
        SchemaDefinition schema,
        IResolverContext context)
    {
        var input = context.ArgumentValue<Dictionary<string, object>>("input");
        input.InjectDefaultValueOnInsert();

        var document = new BsonDocument(
            input.Select(kv =>
            {
                var value = kv.Value is DateTimeOffset dto ? dto.UtcDateTime : kv.Value;
                return new BsonElement(kv.Key, BsonValue.Create(value));
            }));
            //BsonDocument.Create(input);
        await _repository.InsertAsync(schema.CollectionName, document);

        var actionResponse = new ActionResponse
        {
            Acknowledged = true,
            ItemId = document[GraphQLConstant.DbEntityIdFieldName].ToString()
        };
        return actionResponse;
    }
    public async Task<ActionResponse> UpdateAsync(
        SchemaDefinition schema,
        IResolverContext context)
    {
        var filterJson = context.ArgumentValue<string>("filter");

        var inputLiteral = context.ArgumentLiteral<IValueNode>("input") as ObjectValueNode;

        var input = inputLiteral?.Fields.ToDictionary(
            f => f.Name.Value,
            f => f.Value.ParseValueNode()
        ) ?? new Dictionary<string, object>();
        
        input.InjectDefaultValueOnUpdate();
        
        var filter = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(filterJson);

        var update = new BsonDocument(
            input.Select(kv =>
            {
                var value = kv.Value is DateTimeOffset dto ? dto.UtcDateTime : kv.Value;
                return new BsonElement(kv.Key, BsonValue.Create(value));

            }));

        return await _repository.UpdateAsync(schema.CollectionName, filter, update);

    }
    public async Task<ActionResponse> DeleteAsync(
        SchemaDefinition schema,
        IResolverContext context)
    {
        var filterJson = context.ArgumentValue<string>("filter");
        var filter = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(filterJson);
        return await _repository.DeleteAsync(schema.CollectionName, filter);
    }
    
}