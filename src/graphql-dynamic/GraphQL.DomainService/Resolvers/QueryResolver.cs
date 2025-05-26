using System;
using System.Threading.Tasks;
using GraphQL.DomainService.Enities;
using GraphQL.DomainService.GraphTypes;
using GraphQL.DomainService.Models;
using GraphQL.DomainService.Repositories;
using HotChocolate.Resolvers;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GraphQL.DomainService.Resolvers;

public class QueryResolver
{
    private readonly IRepository<BsonDocument> _repository;
    public QueryResolver(IRepository<BsonDocument> repository)
    {
        _repository = repository;
    }
    public async Task<QueryResponse<Dictionary<string, object>>> ResolveAsync(
        IResolverContext ctx,
        SchemaDefinition schema)
    {
        var input = ctx.ArgumentValue<DynamicQueryInput>("input") ?? new();
        var filter = string.IsNullOrWhiteSpace(input.Filter)
                    ? FilterDefinition<BsonDocument>.Empty
                    : MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(input.Filter);
        GetSelectionFields(ctx);
        var sort = input.Sort != null
            ? MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(input.Sort)
            : null;

        var skip = 0;
        var limit = 10;
        if (input.PageNo.HasValue && input.PageSize.HasValue)
        {
            skip = (input.PageNo.Value - 1) * input.PageSize.Value;
            limit = input.PageSize.Value;
        }

        var result = await _repository.GetItemsWithCountAsync(
            schema.CollectionName,
            filter,
            sort,
            null,
            skip,
            limit);
        var items = result.items.Select(d => d.ToDictionary()).ToList();

        return new QueryResponse<Dictionary<string, object>>
        {
            Items = items,
            TotalCount = (int)result.count,
            PageNo = input.PageNo ?? 1,
            PageSize = input.PageSize ?? items.Count
        };
    }
    public void ResolveSchema(
        IObjectTypeDescriptor descriptor,
        SchemaDefinition schema,
        Dictionary<string, QueryOutputType> dynamicTypes)
    {
        var fieldName = schema.SchemaName + "s";
        var responseType = new QueryResponseType(schema.SchemaName, dynamicTypes[schema.SchemaName]);

        var resolver = async (IResolverContext ctx) =>
            await ResolveAsync(ctx, schema);

        descriptor.Field(fieldName)
            .Argument("input", a => a.Type<QueryInputType>())
            .Type(responseType)
            .Resolve(resolver);
    }
    private IEnumerable<string> GetSelectionFields(IResolverContext context)
    {
        var fields = new List<string>();
        var selectionSet = context.Selection.SelectionSet;

        if (selectionSet != null)
        {
            foreach (var selection in selectionSet.Selections)
            {
                if (selection is HotChocolate.Language.FieldNode fieldNode)
                {
                    fields.Add(fieldNode.Name.Value);
                }
            }
        }

        return fields;
    }

}
