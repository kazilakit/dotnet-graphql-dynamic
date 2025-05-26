using System;

namespace GraphQL.DomainService.Models.Responses;

public class ServiceResponse<T>
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public ServiceResponse()
    {

    }
    public ServiceResponse(T data)
    {
        IsSuccess = true;
        Data = data;
    }

}
