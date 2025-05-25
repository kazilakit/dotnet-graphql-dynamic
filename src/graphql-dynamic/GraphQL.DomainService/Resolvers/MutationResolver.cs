using System;
using GraphQL.DomainService.Enities;
using GraphQL.DomainService.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GraphQL.DomainService.Resolvers;

public class MutationResolver
{
    private readonly IMongoDatabase _mongoDb;
    public MutationResolver(IMongoDatabase mongoDb)
    {
        _mongoDb = mongoDb;
    }
    public void ResolveInsertSchema(
    IObjectTypeDescriptor descriptor,
    SchemaDefinition schema,
    InputObjectType inputType)
    {
        var fieldName = "insert" + schema.CollectionName;

        descriptor.Field(fieldName)
            .Argument("input", a => a.Type(inputType))
            .Type<ObjectType<InsertResponse>>()
            .Resolve(async ctx =>
            {
                var input = ctx.ArgumentValue<Dictionary<string, object>>("input");
                var collection = _mongoDb.GetCollection<BsonDocument>(schema.CollectionName);

                var document = BsonDocument.Create(input);
                await collection.InsertOneAsync(document);

                return new InsertResponse
                {
                    Acknowledged = true,
                    InsertedId = document["_id"].ToString()
                };
            });
    }

    public void ResolveUpdateSchema(
        IObjectTypeDescriptor descriptor,
        SchemaDefinition schema,
        InputObjectType inputType)
    {
        var fieldName = "update" + schema.CollectionName;

        descriptor.Field(fieldName)
            .Argument("filter", a => a.Type<StringType>())
            .Argument("input", a => a.Type(inputType))
            .Type<ObjectType<UpdateResponse>>()
            .Resolve(async ctx =>
            {
                var filterJson = ctx.ArgumentValue<string>("filter");
                var input = ctx.ArgumentValue<Dictionary<string, object>>("input");

                var collection = _mongoDb.GetCollection<BsonDocument>(schema.CollectionName);
                var filter = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(filterJson);

                var update = new BsonDocument("$set", new BsonDocument(
                    input.Select(kv => new BsonElement(kv.Key, BsonValue.Create(kv.Value)))));

                var result = await collection.UpdateOneAsync(filter, update);

                return new UpdateResponse
                {
                    Acknowledged = result.IsAcknowledged,
                    TotalUpdated = result.ModifiedCount
                };
            });
    }

    public void ResolveDeleteSchema(
        IObjectTypeDescriptor descriptor,
        SchemaDefinition schema)
    {
        var fieldName = "delete" + schema.CollectionName;

        descriptor.Field(fieldName)
            .Argument("filter", a => a.Type<StringType>())
            .Type<ObjectType<DeleteResponse>>()
            .Resolve(async ctx =>
            {
                var filterJson = ctx.ArgumentValue<string>("filter");
                var filter = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(filterJson);

                var collection = _mongoDb.GetCollection<BsonDocument>(schema.CollectionName);
                var result = await collection.DeleteManyAsync(filter);

                return new DeleteResponse
                {
                    Acknowledged = result.IsAcknowledged,
                    TotalDeleted = result.DeletedCount
                };
            });
    }


}
