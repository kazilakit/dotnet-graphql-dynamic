using GraphQL.DomainService.Enities;
using GraphQL.DomainService.Helpers;
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
        descriptor.Name(_schema.SchemaName);

        descriptor.Field("_id")
            .Type<IdType>()
            .Resolve(ctx => ((IDictionary<string, object>)ctx.Parent<object>()).TryGetValue("_id", out var value) ? value : null);

        foreach (var field in _schema.Fields)
        {
            if (GraphQLTypeHelper.IsScalar(field.Type))
            {
                descriptor.Field(field.Name)
                    .Type(GraphQLTypeHelper.GetTypeNode(field.Type, field.IsArray))
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
                        .Type(GraphQLTypeHelper.GetCustomTypeNode(field.Type, true))
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
                        .Type(GraphQLTypeHelper.GetCustomTypeNode(field.Type))
                        .Resolve(ctx => ((IDictionary<string, object>)ctx.Parent<object>()).TryGetValue(field.Name, out var value) ? value : null);
                }
            }
        }
    }
}


