using GraphQL.DomainService.Entities;
using GraphQL.DomainService.Models;
using HotChocolate.Resolvers;

namespace GraphQL.DomainService.Services;

public interface IMutationService
{
    Task<ActionResponse> InsertAsync(
        SchemaDefinition schema,
        IResolverContext context);
    
    Task<ActionResponse> UpdateAsync(
        SchemaDefinition schema,
        IResolverContext context);

    Task<ActionResponse> DeleteAsync(
        SchemaDefinition schema,
        IResolverContext context);
}