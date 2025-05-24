using GraphQL.DomainService.Enities;
using HotChocolate.Language;

namespace GraphQL.DomainService.GraphTypes;

public class QueryOutputType : ObjectType<object>
{
    private readonly SchemaDefinition _schema;
    private readonly Dictionary<string, SchemaDefinition> _schemaMap;

    public QueryOutputType(SchemaDefinition schema, Dictionary<string, SchemaDefinition> schemaMap)
    {
        _schema = schema;
        _schemaMap = schemaMap;
    }

    protected override void Configure(IObjectTypeDescriptor<object> descriptor)
    {
        descriptor.Name(_schema.CollectionName);

        descriptor.Field("_id")
            .Type<IdType>()
            .Resolve(ctx => ((IDictionary<string, object>)ctx.Parent<object>()).TryGetValue("_id", out var value) ? value : null);

        foreach (var field in _schema.Fields)
        {
            if (IsScalar(field.Type))
            {
                descriptor.Field(field.Name)
                    .Type(GetOutputType(field.Type, field.IsArray))
                    //.Resolve(ctx => ((IDictionary<string, object>)ctx.Parent<object>()).TryGetValue(field.Name, out var value) ? value : null);
                    .Resolve(ctx =>
                    {
                        var parent = ctx.Parent<object>();
                        if (parent is IDictionary<string, object> dict)
                        {
                            return dict.TryGetValue(field.Name, out var value) ? value : null;
                        }
                        return null;
                    });
            }
            else if (_schemaMap.TryGetValue(field.Type, out var nestedSchema))
            {
                if (field.IsArray)
                {
                    descriptor.Field(field.Name)
                        .Type(GetCustomType(field.Type, true))
                        //.Resolve(ctx => ((IDictionary<string, object>)ctx.Parent<object>()).TryGetValue(field.Name, out var value) ? value : null);
                        .Resolve(ctx =>
                                {
                                    var parent = ctx.Parent<object>();
                                    if (parent is IDictionary<string, object> dict)
                                    {
                                        return dict.TryGetValue(field.Name, out var value) ? value : null;
                                    }
                                    return null;
                                });
                }
                else
                {
                    descriptor.Field(field.Name)
                        .Type(GetCustomType(field.Type))
                        .Resolve(ctx => ((IDictionary<string, object>)ctx.Parent<object>()).TryGetValue(field.Name, out var value) ? value : null);
                }
            }
        }
    }

    private static bool IsScalar(string type) =>
        type is "String" or "Int" or "Float" or "Boolean" or "ID";

    private static ITypeNode GetOutputType(string type, bool isArray = false)
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
    private static ITypeNode GetCustomType(string type, bool isArray = false)
    {
        if (isArray)
        {
            return new ListTypeNode(new NamedTypeNode(type)); // Assuming type is a valid GraphQL type name
        }
        return new NamedTypeNode(type); // Assuming type is a valid GraphQL type name
    }
}

public class DynamicObjectTypeReference : ObjectType
{
    private readonly string _typeName;

    public DynamicObjectTypeReference(string typeName)
    {
        _typeName = typeName;
    }

    protected override void Configure(IObjectTypeDescriptor descriptor)
    {
        descriptor.Name(_typeName);
    }
}


