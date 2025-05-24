using MongoDB.Bson;

namespace GraphQL.DomainService.Enities;

public class SchemaDefinition
{
    public ObjectId Id { get; set; }
    public string CollectionName { get; set; }
    public List<FieldDefinition> Fields { get; set; }
    public string SchemaName { get; set; }
    public bool SchemaOnly { get; set; }

}
