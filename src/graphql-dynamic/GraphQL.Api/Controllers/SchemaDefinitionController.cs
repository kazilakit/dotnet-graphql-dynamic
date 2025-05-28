using GraphQL.DomainService.Models;
using GraphQL.DomainService.Services;
using Microsoft.AspNetCore.Mvc;

namespace GraphQL.Api.Controllers;

[Route("api/schema/definition")]
[ApiController]
public class SchemaDefinitionController : ControllerBase
{
    private readonly ISchemaDefinitionService _service;
    public SchemaDefinitionController(ISchemaDefinitionService service)
    {
        _service = service;
    }
    [HttpGet]
    public async Task<IActionResult> GetSchemas([FromQuery] GetSchemaDefinitionListRequest request)
    {
        var schemas = await _service.GetAllSchemasAsync(request);
        return Ok(schemas);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSchemaById(string id)
    {
        var response = await _service.GetSchemaByIdAsync(id);
        if (response.IsSuccess)
        {
            return Ok(response);
        }
        return NotFound(response);
    }
    [HttpPost]
    public async Task<IActionResult> CreateSchema([FromBody] CreateSchemaDefinitionRequest request)
    {
        var response = await _service.CreateSchemaAsync(request);
        if (response.IsSuccess)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }
    [HttpPut]
    public async Task<IActionResult> UpdateSchema([FromBody] UpdateSchemaDefinitionRequest request)
    {
        var response = await _service.UpdateSchemaAsync(request);
        if (response.IsSuccess)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSchema(string id)
    {
        var response = await _service.DeleteSchemaAsync(id);
        if (response.IsSuccess)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

}


