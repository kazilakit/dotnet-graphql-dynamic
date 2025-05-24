using System;

namespace GraphQL.DomainService.Models;

public class DeleteResponse
{
    public bool Acknowledged { get; set; }
    public long TotalDeleted { get; set; }
}