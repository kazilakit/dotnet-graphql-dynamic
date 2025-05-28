using MongoDB.Bson.Serialization.Attributes;

namespace GraphQL.DomainService.Entities;

public class BaseEntity
{
    [BsonId]
    public string ItemId { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}