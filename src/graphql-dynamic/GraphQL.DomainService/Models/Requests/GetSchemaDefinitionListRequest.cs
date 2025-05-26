using System;

namespace GraphQL.DomainService.Models;

public class GetSchemaDefinitionListRequest : BasePaginationRequest
{
    public string Keyword { get; set; } = string.Empty;
}
