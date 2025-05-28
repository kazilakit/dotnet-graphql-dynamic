using System;

namespace GraphQL.DomainService.Entities;

public class FieldDefinition
{
    public string Name { get; set; }
    public string Type { get; set; }
    public bool IsArray { get; set; }
}