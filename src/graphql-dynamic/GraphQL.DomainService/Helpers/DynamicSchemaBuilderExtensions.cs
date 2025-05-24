using System.Linq;
using GraphQL.DomainService.Enities;
using GraphQL.DomainService.GraphTypes;
using GraphQL.DomainService.Models;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;


namespace GraphQL.DomainService.Helpers;

public static class DynamicSchemaBuilderExtensions
{
    public static async Task BuildDynamicSchema(IServiceProvider services, ISchemaBuilder schemaBuilder, CancellationToken cancellationToken)
    {
        var mongoDb = services.GetRequiredService<IMongoDatabase>();
        var schemas = await LoadSchemaDefinitions(mongoDb, cancellationToken);
        var customTypes = schemas.ToDictionary(s => s.CollectionName, s => s);

        var outputTypes = schemas.ToDictionary(
        s => s.CollectionName,
        s => new DynamicObjectType(s, customTypes));

        var insertInputTypes = schemas.ToDictionary(
            s => s.CollectionName,
            s => new DynamicInsertInputType(s, customTypes));

        var updateInputTypes = schemas.ToDictionary(
            s => s.CollectionName,
            s => new DynamicUpdateInputType(s, customTypes));

        var queryType = BuildQueryType(mongoDb, schemas, outputTypes);
        var mutationType = BuildMutationType(mongoDb, schemas, insertInputTypes, updateInputTypes);

        schemaBuilder.AddQueryType(queryType);
        schemaBuilder.AddMutationType(mutationType);
    }

    private static async Task<List<SchemaDefinition>> LoadSchemaDefinitions(IMongoDatabase db, CancellationToken cancellationToken)
    {
        var collection = db.GetCollection<SchemaDefinition>("schemas");
        return await collection.Find(FilterDefinition<SchemaDefinition>.Empty).SortByDescending(x => x.SchemaOnly).ToListAsync(cancellationToken);
    }

    private static ObjectType BuildQueryType(
        IMongoDatabase mongoDb,
        IEnumerable<SchemaDefinition> schemas,
        Dictionary<string, DynamicObjectType> dynamicTypes)
    {
        return new ObjectType(descriptor =>
        {
            descriptor.Name("Query");

            foreach (var schema in schemas)
            {
                ResolveSchema(mongoDb, descriptor, schema, dynamicTypes);
            }
        });
    }

    private static ObjectType BuildMutationType(
        IMongoDatabase mongoDb,
        IEnumerable<SchemaDefinition> schemas,
        Dictionary<string, DynamicInsertInputType> insertInputTypes,
        Dictionary<string, DynamicUpdateInputType> updateInputTypes)
    {
        return new ObjectType(descriptor =>
        {
            descriptor.Name("Mutation");

            foreach (var schema in schemas)
            {
                ResolveInsertSchema(mongoDb, descriptor, schema, insertInputTypes[schema.CollectionName]);
                ResolveUpdateSchema(mongoDb, descriptor, schema, updateInputTypes[schema.CollectionName]);
                ResolveDeleteSchema(mongoDb, descriptor, schema);
            }
        });
    }

    private static void ResolveInsertSchema(
        IMongoDatabase mongoDb,
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
                var collection = mongoDb.GetCollection<BsonDocument>(schema.CollectionName);

                var document = BsonDocument.Create(input);
                await collection.InsertOneAsync(document);

                return new InsertResponse
                {
                    Acknowledged = true,
                    InsertedId = document["_id"].ToString()
                };
            });
    }

    private static void ResolveUpdateSchema(
        IMongoDatabase mongoDb,
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

                var collection = mongoDb.GetCollection<BsonDocument>(schema.CollectionName);
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

    private static void ResolveDeleteSchema(
        IMongoDatabase mongoDb,
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

                var collection = mongoDb.GetCollection<BsonDocument>(schema.CollectionName);
                var result = await collection.DeleteManyAsync(filter);

                return new DeleteResponse
                {
                    Acknowledged = result.IsAcknowledged,
                    TotalDeleted = result.DeletedCount
                };
            });
    }

    private static void ResolveSchema(
        IMongoDatabase mongoDb,
        IObjectTypeDescriptor descriptor,
        SchemaDefinition schema,
        Dictionary<string, DynamicObjectType> dynamicTypes)
    {
        var fieldName = schema.CollectionName.ToLower() + "s";
        var responseType = new QueryResponseType(schema.CollectionName, dynamicTypes[schema.CollectionName]);

        descriptor.Field(fieldName)
            .Argument("input", a => a.Type<DynamicQueryInputType>())
            .Type(responseType)
            .Resolve(async ctx =>
            {
                var input = ctx.ArgumentValue<DynamicQueryInput>("input") ?? new();
                var collection = mongoDb.GetCollection<BsonDocument>(schema.CollectionName);

                var filter = string.IsNullOrWhiteSpace(input.Filter)
                    ? FilterDefinition<BsonDocument>.Empty
                    : MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(input.Filter);

                var find = collection.Find(filter);
                var totalCount = await find.CountDocumentsAsync();

                if (!string.IsNullOrWhiteSpace(input.Sort))
                {
                    var sortDef = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(input.Sort);
                    var sortBuilder = Builders<BsonDocument>.Sort;
                    SortDefinition<BsonDocument>? sort = null;

                    foreach (var element in sortDef.Elements)
                    {
                        var direction = element.Value.ToString().ToLower() == "desc"
                            ? sortBuilder.Descending(element.Name)
                            : sortBuilder.Ascending(element.Name);

                        sort = sort == null ? direction : sort;//.Combine(direction);
                    }

                    find = find.Sort(sort);
                }

                if (input.PageNo.HasValue && input.PageSize.HasValue)
                {
                    var skip = (input.PageNo.Value - 1) * input.PageSize.Value;
                    find = find.Skip(skip).Limit(input.PageSize.Value);
                }

                var docs = await find.ToListAsync();
                var items = docs.Select(d => d.ToDictionary()).ToList();

                return new QueryResponse<Dictionary<string, object>>
                {
                    Items = items,
                    TotalCount = (int)totalCount,
                    PageNo = input.PageNo ?? 1,
                    PageSize = input.PageSize ?? items.Count
                };
            });
    }
}


