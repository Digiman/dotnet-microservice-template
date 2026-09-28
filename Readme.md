# Simple Service Template for .NET application

[![CI](https://github.com/Digiman/dotnet-microservice-template/actions/workflows/ci.yml/badge.svg)](https://github.com/Digiman/dotnet-microservice-template/actions/workflows/ci.yml)
[![Lint Code Base](https://github.com/Digiman/dotnet-microservice-template/actions/workflows/super-linter.yml/badge.svg)](https://github.com/Digiman/dotnet-microservice-template/actions/workflows/super-linter.yml)
[![CodeQL](https://github.com/Digiman/dotnet-microservice-template/actions/workflows/codeql.yml/badge.svg)](https://github.com/Digiman/dotnet-microservice-template/actions/workflows/codeql.yml)

## Introduction

This repository is a template for a .NET microservice. It contains a single Web API project
(`DotNet.ServiceName.Api`) with a layered structure (Application, Common), API versioning,
API Key authentication, Swagger/OpenAPI documentation, structured logging, health checks with
a dashboard, and a Docker setup for local development.

### Project structure

```
src/
  DotNet.ServiceName.Api/          # Web API host: controllers, middleware, Swagger/Scalar, auth
  DotNet.ServiceName.Application/  # Business logic, DTOs/facets, service registrations
  DotNet.ServiceName.Common/       # Shared configuration options and extension helpers
tests/
  DotNet.ServiceName.Application.Tests/  # xUnit tests (services, mappings, DI)
```

Rename `DotNet.ServiceName` to your service name across the solution, project folders,
namespaces, and the `Constants.ApiName` value when using the template.

## Tech stack

Application developed and used next technologies (on the backend) and components:

* .NET 10 (LTS) - see [`global.json`](global.json) for the pinned SDK version
* API Key authentication (custom handler) with Swagger UI / Scalar integration
* [Serilog](https://github.com/serilog/serilog) for logging
* [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) for Swagger (OpenAPI)
* [Scalar](https://scalar.com/) for an alternative interactive API reference UI ([Scalar.AspNetCore](https://www.nuget.org/packages/Scalar.AspNetCore))
* [Asp.Versioning](https://github.com/dotnet/aspnet-api-versioning) for API versioning (URL segment based)
* [Facet](https://github.com/Tim-Maes/Facet) for compile-time generated DTOs and mapping (no runtime reflection), with [Facet.Extensions](https://www.nuget.org/packages/Facet.Extensions) helpers (`ToFacet`) and a [Facet.Dashboard](https://www.nuget.org/packages/Facet.Dashboard) page (`/facets`) to inspect all facets
* HealthCheck UI for ASP.NET Core - [DotNetDiag HealthChecks for ASP.NET Core Diagnostics Package](https://github.com/DotNetDiag/HealthChecks)
* Central Package Management via [`Directory.Packages.props`](Directory.Packages.props)

## Logging

Service/web application use Serilog to write and generate structure logs with details how application working. It's possible to configure logs to send to the different services like Splunk to monitor in one single place or use other tools to read the logs. Depending on hosting type and where the service wil be placed.

## Authentication (API Key)

Secured endpoints require an API key sent in an HTTP header (`X-API-Key` by default).
The scheme is implemented in `ApiKeyAuthenticationHandler` and applied via
`[Authorize(AuthenticationSchemes = ApiKeyAuthenticationHandler.SchemeName)]` on
controllers. Health check endpoints stay anonymous.

Configuration lives in the `ApiKeyOptions` section of `appsettings.json`:

```json
"ApiKeyOptions": {
  "HeaderName": "X-API-Key",
  "ApiKey": "local-dev-api-key"
}
```

Key points:

* The key is validated with a constant-time comparison (`CryptographicOperations.FixedTimeEquals`).
* Missing/invalid key returns `401 Unauthorized`; a valid key is required for all secured endpoints.
* `SwaggerAuth` (`true` in `appsettings.json`) exposes the security scheme in Swagger UI
  (Authorize button) and Scalar (persistent authentication) so you can call secured endpoints
  from both UIs without leaving them.
* Options are validated on startup (DataAnnotations; FluentValidation variant is available via
  `AddConfigurationWithFluentValidation`).
* For real environments, override the key via environment variables
  (`ApiKeyOptions__ApiKey`) or user secrets instead of committing it to source control.

## Monitoring

No any monitoring tools/services are available in the service at this time.

## Availability and Health check

The service exposes the following health check endpoints:

| Endpoint | Description |
|---|---|
| `/healthcheck` | Simple readiness probe (checks tagged `ready`) |
| `/health` | Full health report with details |
| `/health/ready` | Readiness endpoint |
| `/health/live` | Liveness endpoint |
| `/healthcheck-dashboard` | Health Checks UI dashboard (when enabled in configuration) |

## Security headers

Non-development environments get a set of security headers applied by
[NetEscapades.AspNetCore.SecurityHeaders](https://github.com/andrewlock/NetEscapades.AspNetCore.SecurityHeaders):
HSTS (365 days, subdomains included), CSP, `X-Frame-Options`, `X-Content-Type-Options`,
`Referrer-Policy`, Permissions-Policy, and Cross-Origin policies. See
`ApplicationBuilderExtensions.ConfigureSecurityHeaders` for the configured policy.

## CORS

Cross-origin requests are allowed for the origins listed in configuration. The policy answers
preflight (`OPTIONS`) requests before authentication, so browser clients can send the API key
header without an extra round trip:

```json
"CorsPolicyOptions": {
  "Enabled": true,
  "AllowedOrigins": [ "http://localhost:4200", "http://localhost:3000" ],
  "AllowCredentials": false
}
```

HTTP methods and allowed headers default to the common REST verbs and `*` - see
`CorsPolicyOptions` for the full list of settings.

## Rate limiting

A global fixed-window limiter protects the API from request floods, partitioned by the client
IP address (after forwarded headers are processed). Requests over the limit get `429 Too Many
Requests` with a ProblemDetails body and a `Retry-After` header. Health check endpoints are
exempt so monitoring probes are never throttled:

```json
"RateLimitingOptions": {
  "Enabled": true,
  "PermitLimit": 100,
  "WindowSeconds": 60,
  "QueueLimit": 0
}
```

## Request timeouts

Every request is bounded by a default timeout - slow or stuck handlers are aborted with
`503 Service Unavailable` instead of holding connections open:

```json
"HttpTimeoutOptions": {
  "DefaultTimeoutSeconds": 30
}
```

Per-endpoint overrides can be added later with the `[RequestTimeout]` attribute.

## Running behind a proxy / load balancer

Forwarded headers are processed, but only a loopback proxy is trusted by default so clients
cannot spoof `X-Forwarded-*`. When running behind a reverse proxy or load balancer, list its
IP address in configuration:

```json
"ForwardedHeaders": {
  "KnownProxies": ["10.0.0.5"]
}
```

## Build Process for Local Development

* You have Docker installed - ideally latest version of the tool.
* You have .NET 10 installed (SDK and runtime). The required version is pinned in
  [`global.json`](global.json); run `dotnet --version` inside the repository to verify it resolves.
* Visual Studio 2022 (17.14+) or JetBrains Rider (2024.1+) or Visual Studio Code as IDE - one of them, better for you, all them is appropriate.

### Common commands

```bash
# restore + build + test
dotnet build DotNet.ServiceName.sln -c Release
dotnet test DotNet.ServiceName.sln -c Release

# format / code style check (CI-friendly)
dotnet format DotNet.ServiceName.sln --verify-no-changes

# build and run in Docker (container listens on port 8080 internally)
docker compose up --build
# then open http://localhost:5050/swagger/index.html or http://localhost:5050/scalar

# call a secured endpoint (default local key)
curl -H "X-API-Key: local-dev-api-key" http://localhost:5050/api/v1/values
```

## Links

1. [Download .NET](https://dotnet.microsoft.com/en-us/download)
