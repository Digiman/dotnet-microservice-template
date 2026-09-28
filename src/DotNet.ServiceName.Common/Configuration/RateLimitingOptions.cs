using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace DotNet.ServiceName.Common.Configuration;

/// <summary>
/// Configuration for the global fixed-window rate limiting policy.
/// </summary>
public sealed class RateLimitingOptions
{
    /// <summary>
    /// Is the rate limiting enabled?
    /// </summary>
    [Required]
    public bool Enabled { get; set; }

    /// <summary>
    /// Maximum number of requests allowed per window per client.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PermitLimit { get; set; } = 100;

    /// <summary>
    /// Length of the fixed window (in seconds).
    /// </summary>
    [Range(1, int.MaxValue)]
    public int WindowSeconds { get; set; } = 60;

    /// <summary>
    /// Maximum number of queued requests waiting for a permit - 0 rejects immediately.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int QueueLimit { get; set; }
}

/// <summary>
/// Custom validator for RateLimitingOptions with FluentValidator.
/// </summary>
public sealed class RateLimitingOptionsValidator : AbstractValidator<RateLimitingOptions>
{
    public RateLimitingOptionsValidator()
    {
        RuleFor(x => x.PermitLimit).GreaterThan(0);
        RuleFor(x => x.WindowSeconds).GreaterThan(0);
        RuleFor(x => x.QueueLimit).GreaterThanOrEqualTo(0);
    }
}