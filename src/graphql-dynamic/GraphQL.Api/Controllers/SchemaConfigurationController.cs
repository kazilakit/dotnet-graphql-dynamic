using GraphQL.DomainService.Models.Responses;
using GraphQL.DomainService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GraphQL.Api.Controllers;

[Route("api/schema/configuration")]
[ApiController]
public class SchemaConfigurationController : ControllerBase
{
    private readonly ISchemaConfigurationService _reloadService;
    public SchemaConfigurationController(ISchemaConfigurationService reloadService)
    {
        _reloadService = reloadService ?? throw new ArgumentNullException(nameof(reloadService));
    }
    [HttpPost("reload")]
    public async Task<IActionResult> ReloadSchemaAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _reloadService.ReloadAsync(cancellationToken);
            return Ok(new ServiceResponse<bool>
            {
                IsSuccess = true,
                Data = true,
                Message = "Schema reloaded successfully."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }
}

