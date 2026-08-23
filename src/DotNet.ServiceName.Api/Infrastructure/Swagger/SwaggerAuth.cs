using DotNet.ServiceName.Api.Infrastructure.Auth;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace DotNet.ServiceName.Api.Infrastructure.Swagger;

/// <summary>
/// Provides the configuration of the API Key authentication support for Swagger and Scalar -
/// to be able to authorize and call secured endpoints from both UIs.
/// </summary>
public static class SwaggerAuth
{
    /// <summary>
    /// Name of the security scheme in OpenAPI documents. Must match the authentication scheme name.
    /// </summary>
    public const string SecuritySchemeName = ApiKeyAuthenticationHandler.SchemeName;

    /// <summary>
    /// Adds the API Key security scheme and requirement to Swagger - shows the Authorize button in Swagger UI.
    /// </summary>
    /// <param name="options">Swagger generation options.</param>
    /// <param name="headerName">Name of the HTTP header with the API key.</param>
    public static void AddApiKeySupport(this SwaggerGenOptions options, string headerName)
    {
        // define the API Key security scheme for OpenAPI documents
        options.AddSecurityDefinition(SecuritySchemeName, new OpenApiSecurityScheme
        {
            Name = headerName,
            Description = $"API Key authentication. Enter the value for the '{headerName}' header.",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey
        });

        // apply the security requirement globally to all operations
        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference(SecuritySchemeName, document),
                new List<string>()
            }
        });
    }

    /// <summary>
    /// Adds the API Key authentication support into Scalar UI.
    /// </summary>
    /// <remarks>The key itself is not prefilled here - enter it once via Auth section in Scalar UI.</remarks>
    /// <param name="options">Scalar options.</param>
    public static void AddApiKeySupport(this ScalarOptions options)
    {
        options
            .AddPreferredSecuritySchemes([SecuritySchemeName])
            .EnablePersistentAuthentication();
    }
}
