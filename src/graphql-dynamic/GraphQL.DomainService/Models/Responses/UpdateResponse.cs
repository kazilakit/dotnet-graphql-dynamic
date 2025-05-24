using System;

namespace GraphQL.DomainService.Models;

public class UpdateResponse
{
    public bool Acknowledged { get; set; }
    public long TotalUpdated { get; set; }
}