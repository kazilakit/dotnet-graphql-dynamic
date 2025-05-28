namespace GraphQL.DomainService.Models;

public class ActionResponse
{
    public bool Acknowledged { get; set; }
    public string ItemId { get; set; } = default!;
    public long TotalImpactedData { get; set; }
}