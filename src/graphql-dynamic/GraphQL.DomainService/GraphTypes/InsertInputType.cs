using GraphQL.DomainService.Enities;
using HotChocolate.Language;
using System.Collections.Concurrent;

namespace GraphQL.DomainService.GraphTypes;

public class InsertInputType : InputObjectType<object>
{
    private readonly SchemaDefinition _schema;
    private readonly Dictionary<string, SchemaDefinition> _schemaMap;

    private static readonly ConcurrentDictionary<string, InputObjectType> _cache = new();

    public InsertInputType(SchemaDefinition schema, Dictionary<string, SchemaDefinition> schemaMap)
    {
        _schema = schema;
        _schemaMap = schemaMap;
    }

    protected override void Configure(IInputObjectTypeDescriptor<object> descriptor)
    {

        descriptor.Name($"{_schema.CollectionName}InsertInput");


        foreach (var field in _schema.Fields)
        {
            var fieldType = field.Type;
            var fieldName = field.Name;

            if (IsScalar(fieldType))
            {
                var inputType = GetInputType(fieldType, field.IsArray);
                descriptor.Field(fieldName).Type(inputType);
            }
            else if (_schemaMap.TryGetValue(fieldType, out var nestedSchema))
            {
                if (field.IsArray)
                {
                    var innerType = GetCustomType($"{field.Type}InsertInput");
                    descriptor.Field(fieldName).Type(new ListTypeNode(innerType));
                }
                else
                {
                    descriptor.Field(fieldName).Type(GetCustomType($"{field.Type}InsertInput"));
                }
            }
        }
    }

    private static bool IsScalar(string type) =>
        type is "String" or "Int" or "Float" or "Boolean" or "ID";

    private static ITypeNode GetInputType(string type, bool isArray = false)
    {
        var innerType = type switch
        {

            "String" => new NamedTypeNode("String"),
            "Int" => new NamedTypeNode("Int"),
            "Float" => new NamedTypeNode("Float"),
            "Boolean" => new NamedTypeNode("Boolean"),
            "ID" => new NamedTypeNode("ID"),
            _ => throw new ArgumentException($"Unknown scalar type: {type}")
        };
        return isArray ? new ListTypeNode(innerType) : innerType;
    }

    private static ITypeNode GetCustomType(string type)
    {
        return new NamedTypeNode(type); // Assuming type is a valid GraphQL type name
    }

}
