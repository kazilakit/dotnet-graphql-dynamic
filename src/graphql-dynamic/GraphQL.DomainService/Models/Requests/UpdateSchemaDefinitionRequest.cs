using System;

namespace GraphQL.DomainService.Models;

public class UpdateSchemaDefinitionRequest : CreateSchemaDefinitionRequest
{
    public string ItemId { get; set; }
}
