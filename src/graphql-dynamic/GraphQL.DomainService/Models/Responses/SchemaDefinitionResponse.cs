using System;

namespace GraphQL.DomainService.Models;

public class SchemaDefinitionResponse
{
    public string Id { get; set; }
    public string CollectionName { get; set; }
    public List<FieldDefinitionResponse> Fields { get; set; }
    public string SchemaName { get; set; }
    public bool SchemaOnly { get; set; }
}

public class FieldDefinitionResponse
{
    public string Name { get; set; }
    public string Type { get; set; }
    public bool IsArray { get; set; }
}
