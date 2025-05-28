using System;
using HotChocolate.Language;

namespace GraphQL.DomainService.Helpers;

public static class GraphQLTypeHelper
{
    public static bool IsScalar(string type) =>
            type is "String" or "Int" or "Float" or "Boolean" or "DateTime" or "ID";

    public static ITypeNode GetTypeNode(string type, bool isArray = false)
    {
        var innerType = type switch
        {
            "String" => new NamedTypeNode("String"),
            "Int" => new NamedTypeNode("Int"),
            "Float" => new NamedTypeNode("Float"),
            "Boolean" => new NamedTypeNode("Boolean"),
            "DateTime" => new NamedTypeNode("DateTime"),
            "ID" => new NamedTypeNode("ID"),
            _ => throw new ArgumentException($"Unknown scalar type: {type}")
        };
        return isArray ? new ListTypeNode(innerType) : innerType;
    }

    public static ITypeNode GetCustomTypeNode(string type)
    {
        return new NamedTypeNode(type);
    }
    public static ITypeNode GetCustomTypeNode(string type, bool isArray)
    {
        if (isArray)
        {
            return new ListTypeNode(new NamedTypeNode(type));
        }
        return new NamedTypeNode(type);
    }
    
    public static object? ParseValueNode(this IValueNode valueNode)
    {
        return valueNode switch
        {
            IntValueNode iv => int.Parse(iv.Value),
            FloatValueNode fv => double.Parse(fv.Value),
            StringValueNode sv => TryParseDateTimeOrString(sv.Value),
            BooleanValueNode bv => bv.Value,
            NullValueNode => null,
            ListValueNode lv => lv.Items.Select(ParseValueNode).ToList(),
            ObjectValueNode ov => ov.Fields.ToDictionary(f => f.Name.Value, f => ParseValueNode(f.Value)),
            _ => throw new NotSupportedException($"Unsupported value node type: {valueNode.GetType().Name}")
        };
    }
    
    private static object TryParseDateTimeOrString(string value)
    {
        if (DateTimeOffset.TryParse(value, out var dto))
            return dto;
        if (DateTime.TryParse(value, out var dt))
            return dt;
        return value;
    }
}
