using GraphQL.DomainService.Entities;
using GraphQL.DomainService.Models;
using GraphQL.DomainService.Models.Constants;
using GraphQL.DomainService.Models.Responses;
using GraphQL.DomainService.Repositories;
using GraphQL.DomainService.Helpers;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GraphQL.DomainService.Services;

public class SchemaDefinitionService : ISchemaDefinitionService
{
    private readonly IRepository<SchemaDefinition> _repository;
    private const string _collectionName = GraphQLConstant.GraphQLSchemaDefinitionCollectionName;

    public SchemaDefinitionService(IRepository<SchemaDefinition> repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<ServiceResponse<ActionResponse>> CreateSchemaAsync(CreateSchemaDefinitionRequest request)
    {
        var schema = new SchemaDefinition
        {
            CollectionName = request.CollectionName,
            Fields = request.Fields?.Select(f => new FieldDefinition
            {
                Name = f.Name,
                Type = f.Type,
                IsArray = f.IsArray
            }).ToList() ?? new List<FieldDefinition>(),
            SchemaName = request.SchemaName,
            SchemaOnly = request.SchemaOnly
        };
        schema.InjectDefaultValue();
        schema.AddDefaultFields();

        var result = await _repository.InsertAsync(_collectionName, schema);

        return new ServiceResponse<ActionResponse>
        {
            Data = new ActionResponse { Acknowledged = true, ItemId = result.ItemId },
            IsSuccess = true
        };
    }

    public async Task<ServiceResponse<ActionResponse>> UpdateSchemaAsync(UpdateSchemaDefinitionRequest request)
    {
        var schema = await _repository.GetItemAsync(_collectionName, request.ItemId);

        if (schema == null)
        {
            return new ServiceResponse<ActionResponse>
            {
                IsSuccess = false,
                Message = "Schema not found"
            };
        }

        schema.CollectionName = request.CollectionName;
        schema.Fields = request.Fields?.Select(f => new FieldDefinition
        {
            Name = f.Name,
            Type = f.Type,
            IsArray = f.IsArray
        }).ToList() ?? [];
        schema.SchemaName = request.SchemaName;
        schema.SchemaOnly = request.SchemaOnly;
        schema.InjectDefaultValue();

        var filter = Builders<SchemaDefinition>.Filter.Eq(s => s.ItemId, schema.ItemId);

        var result = await _repository.UpdateAsync(_collectionName, filter, schema);
        return new ServiceResponse<ActionResponse>
        {
            Data = result,
            IsSuccess = true
        };

    }

    public async Task<ServiceResponse<ActionResponse>> DeleteSchemaAsync(string id)
    {
        var schema = await _repository.GetItemAsync(_collectionName, id);

        if (schema == null)
        {
            return new ServiceResponse<ActionResponse>
            {
                IsSuccess = false,
                Message = "Schema not found"
            };
        }

        var result =  await _repository.DeleteAsync(_collectionName, id);
        return new ServiceResponse<ActionResponse>
        {
            Data = result,
            IsSuccess = true
        };

    }

    public async Task<ServiceResponse<SchemaDefinitionResponse>> GetSchemaByIdAsync(string id)
    {
        var schema = await _repository.GetItemAsync(_collectionName, id);

        if (schema == null)
        {
            return new ServiceResponse<SchemaDefinitionResponse>
            {
                IsSuccess = false,
                Message = "Schema not found"
            };
        }

        return new ServiceResponse<SchemaDefinitionResponse>
        {
            Data = MapToResponse(schema),
            IsSuccess = true
        };
    }

    public async Task<ServiceResponse<PaginationResponse<SchemaDefinitionResponse>>> GetAllSchemasAsync(GetSchemaDefinitionListRequest request)
    {
        var result = await _repository.GetItemsWithCountAsync(_collectionName,
            filter: new BsonDocument(),
            sort: new BsonDocument("SchemaName", 1),
            projection: null,
            skip: (request.PageNo - 1) * request.PageSize,
            limit: request.PageSize);

        // Simple pagination logic
        var total = result.count;
        var items = result.items.Select(schema => MapToResponse(schema));

        return new ServiceResponse<PaginationResponse<SchemaDefinitionResponse>>
        {
            Data = new PaginationResponse<SchemaDefinitionResponse>(total, items),
            IsSuccess = true
        };
    }

    private SchemaDefinitionResponse MapToResponse(SchemaDefinition schema)
    {
        return new SchemaDefinitionResponse
        {
            Id = schema.ItemId.ToString(),
            CollectionName = schema.CollectionName,
            Fields = schema.Fields?.Select(f => new FieldDefinitionResponse
            {
                Name = f.Name,
                Type = f.Type,
                IsArray = f.IsArray
            }).ToList() ?? new List<FieldDefinitionResponse>(),
            SchemaName = schema.SchemaName,
            SchemaOnly = schema.SchemaOnly
        };
    }
}