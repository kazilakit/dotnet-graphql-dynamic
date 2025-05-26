using System;

namespace GraphQL.DomainService.Models;

public class CreateSchemaDefinitionRequest
{
    public string CollectionName { get; set; }
    public List<FieldDefinitionRequest> Fields { get; set; }
    public string SchemaName { get; set; }
    public bool SchemaOnly { get; set; }
}

public class FieldDefinitionRequest
{
    public string Name { get; set; }
    public string Type { get; set; }
    public bool IsArray { get; set; }
}
