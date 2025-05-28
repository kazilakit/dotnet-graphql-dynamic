using GraphQL.DomainService.Entities;
using GraphQL.DomainService.Models.Constants;

namespace GraphQL.DomainService.Helpers;

public static class DefaultValueInjection
{
    public static void AddDefaultFields(this SchemaDefinition schemaDefinition)
    {
        var baseEntityFields = typeof(BaseEntity).GetProperties();
        foreach (var baseEntityField in baseEntityFields)
        {
            schemaDefinition.Fields.Add(new FieldDefinition
            {
                Name = baseEntityField.Name,
                Type = baseEntityField.PropertyType.Name,
                IsArray = baseEntityField.PropertyType.IsArray || baseEntityField.PropertyType.IsGenericType && baseEntityField.PropertyType.GetGenericTypeDefinition() == typeof(List<>)
            });
        }
    }
    public static void InjectDefaultValue<T>(this T entity) where T : BaseEntity
    {
        if (string.IsNullOrWhiteSpace(entity.ItemId))
        {
            entity.ItemId = Guid.NewGuid().ToString();
        }
        
        if (entity.CreatedOn == default)
        {
            entity.CreatedOn = DateTime.UtcNow;
        }
        
        entity.UpdatedOn = DateTime.UtcNow;
        
    }
    
    public static void InjectDefaultValueOnInsert(this Dictionary<string, object?> input)
    {
        if (input.TryGetValue(nameof(BaseEntity.ItemId), out var value))
        {
            var itemId = value is null ? Guid.NewGuid().ToString() :
                string.IsNullOrWhiteSpace(value.ToString()) ? Guid.NewGuid().ToString() : value.ToString();
            input.Add(GraphQLConstant.DbEntityIdFieldName, itemId);
            input.Remove(nameof(BaseEntity.ItemId));
        }
        else
        {
            input.Add(GraphQLConstant.DbEntityIdFieldName, Guid.NewGuid().ToString());
        }
        
        if(!input.ContainsKey(nameof(BaseEntity.CreatedOn)))
        {
            input.Add(nameof(BaseEntity.CreatedOn), DateTime.UtcNow);
        }
        else if (input[nameof(BaseEntity.CreatedOn)] is not DateTime)
        {
            input[nameof(BaseEntity.CreatedOn)] = DateTime.UtcNow;
        }
    }
    
    public static void InjectDefaultValueOnUpdate(this Dictionary<string, object> input)
    {
        if (input.ContainsKey(nameof(BaseEntity.ItemId)))
        {
            input.Remove(nameof(BaseEntity.ItemId));
        }
        if (input.ContainsKey(nameof(BaseEntity.CreatedOn)))
        {
            input.Remove(nameof(BaseEntity.CreatedOn));
        }
        
        if(!input.ContainsKey(nameof(BaseEntity.UpdatedOn)))
        {
            input.Add(nameof(BaseEntity.UpdatedOn), DateTime.UtcNow);
        }
        else if (input[nameof(BaseEntity.UpdatedOn)] is not DateTime)
        {
            input[nameof(BaseEntity.UpdatedOn)] = DateTime.UtcNow;
        }
    }
}