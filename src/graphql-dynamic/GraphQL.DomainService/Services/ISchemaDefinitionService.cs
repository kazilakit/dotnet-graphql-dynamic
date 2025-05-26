

using GraphQL.DomainService.Models;
using GraphQL.DomainService.Models.Responses;

namespace GraphQL.DomainService.Services;

public interface ISchemaDefinitionService
{
    Task<ServiceResponse<InsertResponse>> CreateSchemaAsync(CreateSchemaDefinitionRequest request);
    Task<ServiceResponse<UpdateResponse>> UpdateSchemaAsync(UpdateSchemaDefinitionRequest request);
    Task<ServiceResponse<DeleteResponse>> DeleteSchemaAsync(string id);
    Task<ServiceResponse<SchemaDefinitionResponse>> GetSchemaByIdAsync(string id);
    Task<ServiceResponse<PaginationResponse<SchemaDefinitionResponse>>> GetAllSchemasAsync(GetSchemaDefinitionListRequest request);

}
