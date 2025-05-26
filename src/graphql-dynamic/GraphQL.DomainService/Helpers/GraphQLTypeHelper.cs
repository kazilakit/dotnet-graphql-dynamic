using System;
using HotChocolate.Language;

namespace GraphQL.DomainService.Helpers;

public static class GraphQLTypeHelper
{
    public static bool IsScalar(string type) =>
            type is "String" or "Int" or "Float" or "Boolean" or "ID";

    public static ITypeNode GetTypeNode(string type, bool isArray = false)
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
}
