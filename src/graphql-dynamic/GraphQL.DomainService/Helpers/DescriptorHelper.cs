using System;
using GraphQL.DomainService.Entities;
using HotChocolate.Language;

namespace GraphQL.DomainService.Helpers;

public static class DescriptorHelper
{
    public static void ResolveObjectTypeDescriptor(this IObjectTypeDescriptor descriptor, FieldDefinition field)
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
        else if (field.IsArray)
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

    public static void ResolveInputTypeDescriptor(this IInputObjectTypeDescriptor descriptor, FieldDefinition field)
    {
        var fieldType = field.Type;
        var fieldName = field.Name;

        if (GraphQLTypeHelper.IsScalar(fieldType))
        {
            var inputType = GraphQLTypeHelper.GetTypeNode(fieldType, field.IsArray);
            descriptor.Field(fieldName).Type(inputType);
        }
        else if (field.IsArray)
        {
            var innerType = GraphQLTypeHelper.GetCustomTypeNode($"{field.Type}Input");
            descriptor.Field(fieldName).Type(new ListTypeNode(innerType));
        }
        else
        {
            descriptor.Field(fieldName).Type(GraphQLTypeHelper.GetCustomTypeNode($"{field.Type}Input"));
        }

    }

}
