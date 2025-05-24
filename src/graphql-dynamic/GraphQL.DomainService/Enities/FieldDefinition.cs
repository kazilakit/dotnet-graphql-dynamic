using System;

namespace GraphQL.DomainService.Enities;

public class FieldDefinition
{
    public string Name { get; set; }
    public string Type { get; set; }
    public bool IsArray { get; set; }
}