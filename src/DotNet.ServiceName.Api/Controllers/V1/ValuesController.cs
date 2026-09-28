using Asp.Versioning;
using DotNet.ServiceName.Api.Infrastructure.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;

namespace DotNet.ServiceName.Api.Controllers.V1;

/// <summary>
/// Sample controller to work with test values.
/// </summary>
[ApiController]
[Authorize(AuthenticationSchemes = ApiKeyAuthenticationHandler.SchemeName)]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[SwaggerTag("Sample values controller")]
public sealed class ValuesController : ControllerBase
{
    private readonly string[] _values = ["value1", "value2"];

    /// <summary>
    /// Get list of the values.
    /// </summary>
    /// <returns>Returns the list with values.</returns>
    /// <response code="200">Returns the list of values.</response>
    /// <response code="401">Missing or invalid API key.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public ActionResult<IEnumerable<string>> GetAllValues()
    {
        return Ok(_values);
    }

    /// <summary>
    /// Get specific value by the index
    /// </summary>
    /// <param name="index">Index.</param>
    /// <returns>Returns the value.</returns>
    /// <response code="200">Returns the value.</response>
    /// <response code="401">Missing or invalid API key.</response>
    /// <response code="404">Value with the specified index was not found.</response>
    [HttpGet]
    [Route("{index}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<string> GetValue(int index)
    {
        if (index >= _values.Length)
        {
            return NotFound();
        }

        return Ok(_values[index]);
    }
}