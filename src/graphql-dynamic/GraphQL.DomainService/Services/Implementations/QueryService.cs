using GraphQL.DomainService.Entities;
using GraphQL.DomainService.Models;
using GraphQL.DomainService.Repositories;
using HotChocolate.Resolvers;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GraphQL.DomainService.Services;

public class QueryService : IQueryService
{
    private readonly IRepository<BsonDocument> _repository;
    public QueryService(IRepository<BsonDocument> repository)
    {
        _repository = repository;
    }
    
    [Authorize]    
    public async Task<QueryResponse<Dictionary<string, object>>> GetData(
        IResolverContext ctx,
        SchemaDefinition schema)
    {
        var input = ctx.ArgumentValue<DynamicQueryInput>("input") ?? new();
        var filter = string.IsNullOrWhiteSpace(input.Filter)
            ? FilterDefinition<BsonDocument>.Empty
            : MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(input.Filter);
        var projection = BuildProjection(ctx);
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
            projection,
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
    
    private BsonDocument BuildProjection(IResolverContext context)
    {
        var fields = new BsonDocument();
        var selectionSet = context.Selection.SyntaxNode.SelectionSet;

        if (selectionSet != null)
        {
            foreach (var selection in selectionSet.Selections)
            {
                if (selection is HotChocolate.Language.FieldNode fieldNode && fieldNode.Name.Value == "items")
                {
                    if (fieldNode.SelectionSet != null)
                    {
                        AddProjectionItems(fieldNode.SelectionSet, "", fields);
                    }
                }
            }
        }

        return fields;
    }

    private void AddProjectionItems(
        HotChocolate.Language.SelectionSetNode selectionSet,
        string prefix,
        BsonDocument fields)
    {
        foreach (var selection in selectionSet.Selections)
        {
            if (selection is HotChocolate.Language.FieldNode fieldNode)
            {
                var fieldName = fieldNode.Name.Value;
                var projectionField = string.IsNullOrEmpty(prefix) ? fieldName : $"{prefix}.{fieldName}";
                if (fieldNode.SelectionSet is null)
                {
                    fields.Add(projectionField, 1);
                }
                else
                {
                    AddProjectionItems(fieldNode.SelectionSet, projectionField, fields);
                }
            }
        }
    }
}