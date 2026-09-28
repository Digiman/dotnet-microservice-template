using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace DotNet.ServiceName.Common.Configuration;

/// <summary>
/// Configuration for the default request timeout applied to all endpoints.
/// </summary>
public sealed class HttpTimeoutOptions
{
    /// <summary>
    /// Default timeout for the whole request (in seconds) - default 30s.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int DefaultTimeoutSeconds { get; set; } = 30;
}

/// <summary>
/// Custom validator for HttpTimeoutOptions with FluentValidator.
/// </summary>
public sealed class HttpTimeoutOptionsValidator : AbstractValidator<HttpTimeoutOptions>
{
    public HttpTimeoutOptionsValidator()
    {
        RuleFor(x => x.DefaultTimeoutSeconds).GreaterThan(0);
    }
}