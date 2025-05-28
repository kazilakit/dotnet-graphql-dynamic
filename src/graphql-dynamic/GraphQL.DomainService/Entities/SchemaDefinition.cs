namespace GraphQL.DomainService.Entities;

public class SchemaDefinition : BaseEntity
{
    public string CollectionName { get; set; }
    public List<FieldDefinition> Fields { get; set; }
    public string SchemaName { get; set; }
    public bool SchemaOnly { get; set; }

}
