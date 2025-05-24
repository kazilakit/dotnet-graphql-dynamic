using System.Linq;
using GraphQL.DomainService.Enities;
using GraphQL.DomainService.GraphTypes;
using GraphQL.DomainService.Models;
using GraphQL.DomainService.Resolvers;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;


namespace GraphQL.DomainService;

public static class SchemaBuilderExtensions
{
    public static async Task BuildSchema(IServiceProvider services, ISchemaBuilder schemaBuilder, CancellationToken cancellationToken)
    {
        var mongoDb = services.GetRequiredService<IMongoDatabase>();
        var schemas = await LoadSchemaDefinitions(mongoDb, cancellationToken);
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
        Dictionary<string, QueryOutputType> dynamicTypes)
    {
        return new ObjectType(descriptor =>
        {
            descriptor.Name("Query");

            foreach (var schema in schemas)
            {
                QueryResolver.ResolveSchema(mongoDb, descriptor, schema, dynamicTypes);
            }
        });
    }

    private static ObjectType BuildMutationType(
        IMongoDatabase mongoDb,
        IEnumerable<SchemaDefinition> schemas,
        Dictionary<string, InsertInputType> insertInputTypes,
        Dictionary<string, UpdateInputType> updateInputTypes)
    {
        return new ObjectType(descriptor =>
        {
            descriptor.Name("Mutation");

            foreach (var schema in schemas)
            {
                MutationResolver.ResolveInsertSchema(mongoDb, descriptor, schema, insertInputTypes[schema.CollectionName]);
                MutationResolver.ResolveUpdateSchema(mongoDb, descriptor, schema, updateInputTypes[schema.CollectionName]);
                MutationResolver.ResolveDeleteSchema(mongoDb, descriptor, schema);
            }
        });
    }

}


