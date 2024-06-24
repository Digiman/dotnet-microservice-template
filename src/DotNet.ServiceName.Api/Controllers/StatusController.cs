using System.Threading.Tasks;
using Asp.Versioning;
using AutoMapper;
using DotNet.ServiceName.Application.DTOs;
using DotNet.ServiceName.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DotNet.ServiceName.Api.Controllers;

/// <summary>
/// Controller to get the status of teh application to monitor activity and work, get configuration details and etc.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Produces("application/json")]
[SwaggerTag("Work with statuses and configuration for the service")]
public sealed class StatusController : ControllerBase
{
    private readonly IApplicationStatusService _applicationStatusService;
    private readonly IMapper _mapper;

    public StatusController(IApplicationStatusService applicationStatusService, IMapper mapper)
    {
        _applicationStatusService = applicationStatusService;
        _mapper = mapper;
    }

    /// <summary>
    /// Get status for application.
    /// </summary>
    /// <returns>Returns information about application and it's status.</returns>
    /// <response code="200">Returns information about application.</response>
    /// <response code="500">Server error happened during processing the request.</response>
    [HttpGet]
    [ProducesResponseType(typeof(StatusResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<StatusResponseDto>> GetStatus()
    {
        var status = await _applicationStatusService.GetApplicationStatusAsync();
        var result = _mapper.Map<StatusResponseDto>(status);

        return Ok(result);
    }
}