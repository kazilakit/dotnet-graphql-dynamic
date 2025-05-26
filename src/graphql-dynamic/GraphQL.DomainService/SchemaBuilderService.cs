using System;
using GraphQL.DomainService.Enities;
using GraphQL.DomainService.GraphTypes;
using GraphQL.DomainService.Helpers;
using GraphQL.DomainService.Models.Constants;
using GraphQL.DomainService.Repositories;
using GraphQL.DomainService.Resolvers;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GraphQL.DomainService.Services;

public class SchemaBuilderService
{
    // private readonly IMongoDatabase _mongoDb;
    private readonly IRepository<SchemaDefinition> _schemaRepository;
    private readonly QueryResolver _queryResolver;
    private readonly MutationResolver _mutationResolver;
    public SchemaBuilderService(QueryResolver queryResolver, MutationResolver mutationResolver, IRepository<SchemaDefinition> schemaRepository)
    {
        _queryResolver = queryResolver;
        _mutationResolver = mutationResolver;
        _schemaRepository = schemaRepository;
    }
    public async Task BuildSchema(ISchemaBuilder schemaBuilder, CancellationToken cancellationToken)
    {
        var schemas = await LoadSchemaDefinitions(cancellationToken);
        var dbSchemas = schemas.Where(x => !x.SchemaOnly);
        var onlySchemas = schemas.Where(x => x.SchemaOnly);
        var dbSchemaTypes = schemas.ToDictionary(s => s.SchemaName, s => s);

        var outputTypes = schemas.ToDictionary(
        s => s.SchemaName,
        s => new QueryOutputType(s, dbSchemaTypes));

        var insertInputTypes = schemas.ToDictionary(
            s => s.SchemaName,
            s => new InsertInputType(s, dbSchemaTypes));

        var updateInputTypes = schemas.ToDictionary(
            s => s.SchemaName,
            s => new UpdateInputType(s, dbSchemaTypes));

        BuildOnlySchemaType(schemaBuilder, onlySchemas);

        var queryType = BuildQueryType(dbSchemas, outputTypes);
        var mutationType = BuildMutationType(dbSchemas, insertInputTypes, updateInputTypes);

        schemaBuilder.AddQueryType(queryType);
        schemaBuilder.AddMutationType(mutationType);
    }
    private async Task<List<SchemaDefinition>> LoadSchemaDefinitions(CancellationToken cancellationToken)
    {
        return await _schemaRepository.GetItemsAsync(
            GraphQLConstant.GraphQLSchemaDefinitionCollectionName,
            new BsonDocument(),
            null,
            null, 0, 1000);
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
                if (schema.SchemaOnly)
                    continue;
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
                _mutationResolver.ResolveInsertSchema(descriptor, schema, insertInputTypes[schema.SchemaName]);
                _mutationResolver.ResolveUpdateSchema(descriptor, schema, updateInputTypes[schema.SchemaName]);
                // MutationResolver.ResolveDeleteSchema(_mongoDb, descriptor, schema);
            }
        });
    }
    private void BuildOnlySchemaType(ISchemaBuilder schemaBuilder, IEnumerable<SchemaDefinition> schemas)
    {
        foreach (var schema in schemas)
        {
            schemaBuilder.AddType(ResolveType(schema));
            schemaBuilder.AddType(ResolveInputType(schema));
        }
    }
    private InputObjectType ResolveInputType(SchemaDefinition schemaDefinition)
    {
        return new InputObjectType(descriptor =>
                    {
                        descriptor.Name($"{schemaDefinition.SchemaName}Input");
                        foreach (var field in schemaDefinition.Fields)
                        {
                            // var type = GraphQLTypeHelper.GetTypeNode(field.Type, field.IsArray);
                            // descriptor.Field(field.Name).Type(type);
                            descriptor.ResolveInputTypeDescriptor(field);
                        }
                    });
    }
    private ObjectType ResolveType(SchemaDefinition schemaDefinition)
    {
        return new ObjectType(descriptor =>
                    {
                        descriptor.Name(schemaDefinition.SchemaName);
                        foreach (var field in schemaDefinition.Fields)
                        {
                            // var type = GraphQLTypeHelper.GetTypeNode(field.Type, field.IsArray);
                            // descriptor.Field(field.Name).Type(type);
                            descriptor.ResolveObjectTypeDescriptor(field);
                        }
                    });
    }

}
