using GraphQL.DomainService.Entities;
using GraphQL.DomainService.GraphTypes;
using GraphQL.DomainService.Models;
using GraphQL.DomainService.Services;
using HotChocolate.Resolvers;

namespace GraphQL.DomainService.Resolvers;

public class SchemaResolver
{
    private readonly IMutationService _mutationService;
    private readonly IQueryService _queryService;
    public SchemaResolver(IMutationService mutationService,
        IQueryService queryService)
    {
        _mutationService = mutationService;
        _queryService = queryService;
    }
    
    public void ResolveQuerySchema(
        IObjectTypeDescriptor descriptor,
        SchemaDefinition schema,
        Dictionary<string, QueryOutputType> dynamicTypes)
    {
        var fieldName = schema.SchemaName + "s";
        var responseType = new QueryResponseType(schema.SchemaName, dynamicTypes[schema.SchemaName]);

        descriptor.Field(fieldName)
            .Argument("input", a => a.Type<QueryInputType>())
            .Type(responseType)
            .Resolve(async (IResolverContext ctx) =>
                await _queryService.GetData(ctx, schema));
    }
    
    public void ResolveInsertSchema(
        IObjectTypeDescriptor descriptor,
        SchemaDefinition schema,
        InputObjectType inputType)
    {
        var fieldName = "insert" + schema.SchemaName;

        descriptor.Field(fieldName)
            .Argument("input", a => a.Type(inputType))
            .Type<ObjectType<ActionResponse>>()
            .Resolve(async ctx => await _mutationService.InsertAsync(schema, ctx));
    }

    public void ResolveUpdateSchema(
        IObjectTypeDescriptor descriptor,
        SchemaDefinition schema,
        InputObjectType inputType)
    {
        var fieldName = "update" + schema.SchemaName;

        descriptor.Field(fieldName)
            .Argument("filter", a => a.Type<StringType>())
            .Argument("input", a => a.Type(inputType))
            .Type<ObjectType<ActionResponse>>()
            .Resolve(async ctx => await _mutationService.UpdateAsync(schema, ctx));
    }

    public void ResolveDeleteSchema(
        IObjectTypeDescriptor descriptor,
        SchemaDefinition schema)
    {
        var fieldName = "delete" + schema.SchemaName;

        descriptor.Field(fieldName)
            .Argument("filter", a => a.Type<StringType>())
            .Type<ObjectType<ActionResponse>>()
            .Resolve(async ctx => await _mutationService.DeleteAsync(schema, ctx));
    }
}