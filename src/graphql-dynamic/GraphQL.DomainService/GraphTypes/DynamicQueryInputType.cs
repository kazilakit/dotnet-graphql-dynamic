
using GraphQL.DomainService.Models;

namespace GraphQL.DomainService.GraphTypes;

public class DynamicQueryInputType : InputObjectType<DynamicQueryInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<DynamicQueryInput> descriptor)
    {
        descriptor.Name("DynamicQueryInput");
        descriptor.Field(f => f.Filter).Type<StringType>();
        descriptor.Field(f => f.Sort).Type<StringType>();
        descriptor.Field(f => f.PageNo).Type<IntType>();
        descriptor.Field(f => f.PageSize).Type<IntType>();
    }
}
