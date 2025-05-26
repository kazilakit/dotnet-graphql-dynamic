using System.Collections.Concurrent;
using GraphQL.DomainService.Enities;
using GraphQL.DomainService.Helpers;
using HotChocolate.Language;

namespace GraphQL.DomainService.GraphTypes;

public class UpdateInputType : InputObjectType<object>
{
    private readonly SchemaDefinition _schema;
    private readonly Dictionary<string, SchemaDefinition> _schemaMap;

    private static readonly ConcurrentDictionary<string, InputObjectType> _cache = new();

    public UpdateInputType(SchemaDefinition schema, Dictionary<string, SchemaDefinition> schemaMap)
    {
        _schema = schema;
        _schemaMap = schemaMap;
    }

    protected override void Configure(IInputObjectTypeDescriptor<object> descriptor)
    {
        descriptor.Name($"{_schema.SchemaName}UpdateInput");

        descriptor.Field("_id").Type<StringType>();

        foreach (var field in _schema.Fields)
        {
            var fieldType = field.Type;
            var fieldName = field.Name;

            if (GraphQLTypeHelper.IsScalar(fieldType))
            {
                var inputType = GraphQLTypeHelper.GetTypeNode(fieldType, field.IsArray);
                descriptor.Field(fieldName).Type(inputType);
            }
            else if (_schemaMap.TryGetValue(fieldType, out var nestedSchema))
            {
                if (field.IsArray)
                {
                    var itemType = GraphQLTypeHelper.GetCustomTypeNode($"{field.Type}Input");
                    descriptor.Field(fieldName).Type(new ListTypeNode(itemType));
                }
                else
                {
                    descriptor.Field(fieldName).Type(GraphQLTypeHelper.GetCustomTypeNode($"{field.Type}Input"));
                }
            }
        }
    }
}
