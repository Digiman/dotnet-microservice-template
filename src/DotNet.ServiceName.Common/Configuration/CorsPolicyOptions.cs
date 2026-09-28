using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace DotNet.ServiceName.Common.Configuration;

/// <summary>
/// Configuration for the CORS policy.
/// </summary>
public sealed class CorsPolicyOptions
{
    /// <summary>
    /// Is the cross-origin support enabled?
    /// </summary>
    [Required]
    public bool Enabled { get; set; }

    /// <summary>
    /// List of the origins allowed to make cross-origin requests (e.g. "https://app.example.com").
    /// </summary>
    [Required]
    public string[] AllowedOrigins { get; set; } = [];

    /// <summary>
    /// List of the HTTP methods allowed in cross-origin requests.
    /// </summary>
    public string[] AllowedMethods { get; set; } = ["GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS"];

    /// <summary>
    /// List of the request headers allowed in cross-origin requests - "*" allows any header.
    /// </summary>
    public string[] AllowedHeaders { get; set; } = ["*"];

    /// <summary>
    /// Allow cookies and other credentials to be sent in cross-origin requests.
    /// </summary>
    public bool AllowCredentials { get; set; }
}

/// <summary>
/// Custom validator for CorsPolicyOptions with FluentValidator.
/// </summary>
public sealed class CorsPolicyOptionsValidator : AbstractValidator<CorsPolicyOptions>
{
    public CorsPolicyOptionsValidator()
    {
        RuleFor(x => x.AllowedOrigins).NotEmpty().When(x => x.Enabled);
        RuleFor(x => x.AllowedMethods).NotEmpty().When(x => x.Enabled);
    }
}