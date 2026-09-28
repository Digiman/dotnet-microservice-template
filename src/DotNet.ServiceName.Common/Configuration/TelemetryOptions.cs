using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace DotNet.ServiceName.Common.Configuration;

/// <summary>
/// Configuration for the OpenTelemetry traces and metrics.
/// </summary>
public sealed class TelemetryOptions
{
    /// <summary>
    /// Is the OpenTelemetry instrumentation enabled?
    /// </summary>
    [Required]
    public bool Enabled { get; set; }

    /// <summary>
    /// Logical service name reported as the "service.name" resource attribute.
    /// </summary>
    [Required]
    public string ServiceName { get; set; } = "dotnet-servicename";

    /// <summary>
    /// OTLP endpoint for traces and metrics (e.g. "http://localhost:4317") - when empty,
    /// the standard OTEL_EXPORTER_OTLP_* environment variables are used.
    /// </summary>
    public string? OtlpEndpoint { get; set; }

    /// <summary>
    /// Also print traces and metrics to the console - useful for local debugging.
    /// </summary>
    public bool ConsoleExporter { get; set; }
}

/// <summary>
/// Custom validator for TelemetryOptions with FluentValidator.
/// </summary>
public sealed class TelemetryOptionsValidator : AbstractValidator<TelemetryOptions>
{
    public TelemetryOptionsValidator()
    {
        RuleFor(x => x.ServiceName).NotEmpty().When(x => x.Enabled);
        RuleFor(x => x.OtlpEndpoint)
            .Must(endpoint => Uri.TryCreate(endpoint, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrEmpty(x.OtlpEndpoint));
    }
}