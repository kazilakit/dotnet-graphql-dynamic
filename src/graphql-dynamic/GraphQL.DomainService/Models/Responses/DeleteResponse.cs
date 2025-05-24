using System;

namespace GraphQL.DomainService.Models.Responses;

public class DeleteResponse
{
    public bool Acknowledged { get; set; }
    public long TotalDeleted { get; set; }
}