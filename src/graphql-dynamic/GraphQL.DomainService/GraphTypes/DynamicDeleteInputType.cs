

using GraphQL.DomainService.Enities;

namespace GraphQL.DomainService.GraphTypes;

public class DynamicDeleteInputType : InputObjectType
{
    private readonly SchemaDefinition _schema;

    public DynamicDeleteInputType(SchemaDefinition schema)
    {
        _schema = schema;
    }

    protected override void Configure(IInputObjectTypeDescriptor descriptor)
    {
        descriptor.Name(_schema.CollectionName + "DeleteInput");

        descriptor.Field("filter").Type<NonNullType<StringType>>(); // JSON filter
    }
}
