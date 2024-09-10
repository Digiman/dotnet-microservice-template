using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;

namespace DotNet.ServiceName.Api.Controllers.V1;

/// <summary>
/// Sample controller to work with test values.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[SwaggerTag("Sample values controller")]
public sealed class ValuesController : ControllerBase
{
    private readonly string[] _values = new string[] { "value1", "value2" };

    /// <summary>
    /// Get list of the values.
    /// </summary>
    /// <returns>Returns the list with values.</returns>
    [HttpGet]
    public ActionResult<IEnumerable<string>> GetAllValues()
    {
        return Ok(_values);
    }

    /// <summary>
    /// Get specific value by the index
    /// </summary>
    /// <param name="index">Index.</param>
    /// <returns>Returns the value.</returns>
    [HttpGet]
    [Route("{index}")]
    public ActionResult<string> GetValue(int index)
    {
        if (index >= _values.Length)
        {
            return NotFound();
        }

        return Ok(_values[index]);
    }
}