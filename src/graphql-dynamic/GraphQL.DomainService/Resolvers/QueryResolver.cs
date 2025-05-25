using System;
using System.Threading.Tasks;
using GraphQL.DomainService.Enities;
using GraphQL.DomainService.GraphTypes;
using GraphQL.DomainService.Models;
using HotChocolate.Resolvers;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GraphQL.DomainService.Resolvers;

public class QueryResolver
{
    private readonly IMongoDatabase _mongoDb;
    public QueryResolver(IMongoDatabase mongoDb)
    {
        _mongoDb = mongoDb;
    }
    public async Task<QueryResponse<Dictionary<string, object>>> ResolveAsync(
        IResolverContext ctx,
        SchemaDefinition schema)
    {
        var input = ctx.ArgumentValue<DynamicQueryInput>("input") ?? new();
        var collection = _mongoDb.GetCollection<BsonDocument>(schema.CollectionName);

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
    }
    public void ResolveSchema(
        IObjectTypeDescriptor descriptor,
        SchemaDefinition schema,
        Dictionary<string, QueryOutputType> dynamicTypes)
    {
        var fieldName = schema.CollectionName.ToLower() + "s";
        var responseType = new QueryResponseType(schema.CollectionName, dynamicTypes[schema.CollectionName]);

        var resolver = async (IResolverContext ctx) =>
            await ResolveAsync(ctx, schema);

        descriptor.Field(fieldName)
            .Argument("input", a => a.Type<QueryInputType>())
            .Type(responseType)
            .Resolve(resolver);
    }

}
