using GraphQL.DomainService.Entities;
using GraphQL.DomainService.Models;
using HotChocolate.Resolvers;

namespace GraphQL.DomainService.Services;

public interface IQueryService
{
    Task<QueryResponse<Dictionary<string, object>>> GetData(
        IResolverContext ctx,
        SchemaDefinition schema);
}