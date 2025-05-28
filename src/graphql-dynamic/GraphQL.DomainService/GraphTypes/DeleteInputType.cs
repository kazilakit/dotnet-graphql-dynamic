

using GraphQL.DomainService.Entities;

namespace GraphQL.DomainService.GraphTypes;

public class DeleteInputType : InputObjectType
{
    private readonly SchemaDefinition _schema;

    public DeleteInputType(SchemaDefinition schema)
    {
        _schema = schema;
    }

    protected override void Configure(IInputObjectTypeDescriptor descriptor)
    {
        descriptor.Name(_schema.SchemaName + "DeleteInput");

        descriptor.Field("filter").Type<NonNullType<StringType>>(); // JSON filter
    }
}
