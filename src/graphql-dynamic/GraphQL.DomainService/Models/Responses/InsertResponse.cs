using System;

namespace GraphQL.DomainService.Models;

public class InsertResponse
{
    public bool Acknowledged { get; set; }
    public string InsertedId { get; set; } = default!;
}

