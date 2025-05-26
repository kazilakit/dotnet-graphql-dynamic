using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GraphQL.DomainService.Enities;

public class SchemaDefinition
{
    [BsonId]
    public string ItemId { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public string CollectionName { get; set; }
    public List<FieldDefinition> Fields { get; set; }
    public string SchemaName { get; set; }
    public bool SchemaOnly { get; set; }

}
