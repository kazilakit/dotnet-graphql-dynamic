using System;
using GraphQL.DomainService.Enities;
using GraphQL.DomainService.GraphTypes;
using GraphQL.DomainService.Resolvers;
using MongoDB.Driver;

namespace GraphQL.DomainService.Services;

public class SchemaBuilderService
{
    private readonly IMongoDatabase _mongoDb;
    private readonly QueryResolver _queryResolver;
    private readonly MutationResolver _mutationResolver;
    public SchemaBuilderService(IMongoDatabase mongoDb, QueryResolver queryResolver, MutationResolver mutationResolver)
    {
        _queryResolver = queryResolver;
        _mutationResolver = mutationResolver;
        _mongoDb = mongoDb;
    }
    public async Task BuildSchema(ISchemaBuilder schemaBuilder, CancellationToken cancellationToken)
    {
        var schemas = await LoadSchemaDefinitions(cancellationToken);
        var customTypes = schemas.ToDictionary(s => s.CollectionName, s => s);

        var outputTypes = schemas.ToDictionary(
        s => s.CollectionName,
        s => new QueryOutputType(s, customTypes));

        var insertInputTypes = schemas.ToDictionary(
            s => s.CollectionName,
            s => new InsertInputType(s, customTypes));

        var updateInputTypes = schemas.ToDictionary(
            s => s.CollectionName,
            s => new UpdateInputType(s, customTypes));

        var queryType = BuildQueryType(schemas, outputTypes);
        var mutationType = BuildMutationType(schemas, insertInputTypes, updateInputTypes);

        schemaBuilder.AddQueryType(queryType);
        schemaBuilder.AddMutationType(mutationType);
    }
    private async Task<List<SchemaDefinition>> LoadSchemaDefinitions(CancellationToken cancellationToken)
    {
        var collection = _mongoDb.GetCollection<SchemaDefinition>("schemas");
        return await collection.Find(FilterDefinition<SchemaDefinition>.Empty).SortByDescending(x => x.SchemaOnly).ToListAsync(cancellationToken);
    }

    private ObjectType BuildQueryType(
        IEnumerable<SchemaDefinition> schemas,
        Dictionary<string, QueryOutputType> dynamicTypes)
    {
        return new ObjectType(descriptor =>
        {
            descriptor.Name("Query");

            foreach (var schema in schemas)
            {
                _queryResolver.ResolveSchema(descriptor, schema, dynamicTypes);
            }
        });
    }

    private ObjectType BuildMutationType(
        IEnumerable<SchemaDefinition> schemas,
        Dictionary<string, InsertInputType> insertInputTypes,
        Dictionary<string, UpdateInputType> updateInputTypes)
    {
        return new ObjectType(descriptor =>
        {
            descriptor.Name("Mutation");

            foreach (var schema in schemas)
            {
                _mutationResolver.ResolveInsertSchema(descriptor, schema, insertInputTypes[schema.CollectionName]);
                _mutationResolver.ResolveUpdateSchema(descriptor, schema, updateInputTypes[schema.CollectionName]);
                // MutationResolver.ResolveDeleteSchema(_mongoDb, descriptor, schema);
            }
        });
    }
}
