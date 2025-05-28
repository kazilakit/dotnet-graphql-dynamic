

using GraphQL.DomainService.Models;
using GraphQL.DomainService.Models.Responses;

namespace GraphQL.DomainService.Services;

public interface ISchemaDefinitionService
{
    Task<ServiceResponse<ActionResponse>> CreateSchemaAsync(CreateSchemaDefinitionRequest request);
    Task<ServiceResponse<ActionResponse>> UpdateSchemaAsync(UpdateSchemaDefinitionRequest request);
    Task<ServiceResponse<ActionResponse>> DeleteSchemaAsync(string id);
    Task<ServiceResponse<SchemaDefinitionResponse>> GetSchemaByIdAsync(string id);
    Task<ServiceResponse<PaginationResponse<SchemaDefinitionResponse>>> GetAllSchemasAsync(GetSchemaDefinitionListRequest request);

}
