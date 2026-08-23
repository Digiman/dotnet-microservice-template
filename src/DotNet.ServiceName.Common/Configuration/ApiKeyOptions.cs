using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace DotNet.ServiceName.Common.Configuration;

/// <summary>
/// Configuration for the API Key authentication.
/// </summary>
public sealed class ApiKeyOptions
{
    /// <summary>
    /// Name of the HTTP header used to send the API key by clients.
    /// </summary>
    [Required]
    public required string HeaderName { get; set; }

    /// <summary>
    /// Value of the API key expected from the clients.
    /// </summary>
    [Required]
    public required string ApiKey { get; set; }
}

/// <summary>
/// Custom validator for ApiKeyOptions with FluentValidator.
/// </summary>
public sealed class ApiKeyOptionsValidator : AbstractValidator<ApiKeyOptions>
{
    public ApiKeyOptionsValidator()
    {
        RuleFor(x => x.HeaderName).NotEmpty();
        RuleFor(x => x.ApiKey).NotEmpty();
    }
}
