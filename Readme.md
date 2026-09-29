# Simple Service Template for .NET application

[![CI](https://github.com/Digiman/dotnet-microservice-template/actions/workflows/ci.yml/badge.svg)](https://github.com/Digiman/dotnet-microservice-template/actions/workflows/ci.yml)
[![Lint Code Base](https://github.com/Digiman/dotnet-microservice-template/actions/workflows/super-linter.yml/badge.svg)](https://github.com/Digiman/dotnet-microservice-template/actions/workflows/super-linter.yml)
[![CodeQL](https://github.com/Digiman/dotnet-microservice-template/actions/workflows/codeql.yml/badge.svg)](https://github.com/Digiman/dotnet-microservice-template/actions/workflows/codeql.yml)

## Introduction

This repository is a template for a .NET microservice. It contains a single Web API project
(`DotNet.ServiceName.Api`) with a layered structure (Application, Common), API versioning,
API Key authentication, Swagger/OpenAPI documentation, structured logging, health checks with
a dashboard, CORS, rate limiting, request timeouts, OpenTelemetry traces and metrics, and a
Docker setup for local development.

### Project structure

```
src/
  DotNet.ServiceName.Api/              # Web API host: controllers, Razor home page, middleware, Swagger/Scalar, auth
  DotNet.ServiceName.Application/      # Business logic, DTOs/facets, service registrations
  DotNet.ServiceName.Common/           # Shared configuration options and extension helpers
  DotNet.ServiceName.AppHost/          # Aspire AppHost: local orchestration of the API and Seq (aspire run)
  DotNet.ServiceName.ServiceDefaults/  # Aspire service defaults: OpenTelemetry, service discovery, HttpClient resilience
tests/
  DotNet.ServiceName.Application.Tests/  # xUnit unit tests (services, mappings, DI)
  DotNet.ServiceName.Api.Tests/          # xUnit integration tests (WebApplicationFactory)
```

Rename `DotNet.ServiceName` to your service name across the solution, project folders,
namespaces, and the `Constants.ApiName` value when using the template.

## Tech stack

Application developed and used next technologies (on the backend) and components:

* .NET 10 (LTS) - see [`global.json`](global.json) for the pinned SDK version
* [.NET Aspire](https://aspire.dev/) for local orchestration (AppHost + service defaults)
* API Key authentication (custom handler) with Swagger UI / Scalar integration
* [Serilog](https://github.com/serilog/serilog) for logging
* [OpenTelemetry](https://opentelemetry.io/) for traces and metrics (OTLP export)
* [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) for Swagger (OpenAPI)
* [Scalar](https://scalar.com/) for an alternative interactive API reference UI ([Scalar.AspNetCore](https://www.nuget.org/packages/Scalar.AspNetCore))
* [Asp.Versioning](https://github.com/dotnet/aspnet-api-versioning) for API versioning (URL segment based)
* [Facet](https://github.com/Tim-Maes/Facet) for compile-time generated DTOs and mapping (no runtime reflection), with [Facet.Extensions](https://www.nuget.org/packages/Facet.Extensions) helpers (`ToFacet`) and a [Facet.Dashboard](https://www.nuget.org/packages/Facet.Dashboard) page (`/facets`) to inspect all facets
* HealthCheck UI for ASP.NET Core - [DotNetDiag HealthChecks for ASP.NET Core Diagnostics Package](https://github.com/DotNetDiag/HealthChecks)
* xUnit + `WebApplicationFactory` for unit and integration tests
* Central Package Management via [`Directory.Packages.props`](Directory.Packages.props)

## Logging

Service/web application use Serilog to write and generate structure logs with details how application working. It's possible to configure logs to send to the different services like Splunk to monitor in one single place or use other tools to read the logs. Depending on hosting type and where the service wil be placed. Request log entries carry the `TraceId`, so they can be correlated with the corresponding OpenTelemetry trace.

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

Traces and metrics are exported via OpenTelemetry (OTLP) - see the
[Telemetry](#telemetry-opentelemetry) section. Point the exporter at a collector such as
Jaeger, Grafana Tempo, or an observability platform to store and visualize them.

## Availability and Health check

The service exposes the following health check endpoints:

| Endpoint | Description |
|---|---|
| `/healthcheck` | Simple readiness probe (checks tagged `ready`) |
| `/health` | Full health report with details |
| `/health/ready` | Readiness endpoint |
| `/health/live` | Liveness endpoint |
| `/healthcheck-dashboard` | Health Checks UI dashboard (when enabled in configuration) |

### Dashboard customization

The dashboard is a packaged single page application, so it is themed through a custom style sheet
that is loaded after its own one. The path is configured relative to `wwwroot`:

```json
"HealthCheckOptions": {
  "HeaderText": "Simple Service - Health Checks Status",
  "CustomStylesheet": "css/healthcheck-dashboard.css"
}
```

[`wwwroot/css/healthcheck-dashboard.css`](src/DotNet.ServiceName.Api/wwwroot/css/healthcheck-dashboard.css)
overrides the CSS custom properties the dashboard declares (colors, fonts, surfaces) and follows the
system theme with a dark palette. Every text and background pair it introduces is checked for at
least WCAG AA contrast (4.5:1) in both palettes - keep it that way when changing a color. The logo comes from [`wwwroot/images/healthcheck-logo.svg`](src/DotNet.ServiceName.Api/wwwroot/images/healthcheck-logo.svg)
through the `--logoImageUrl` property - the default logo of the dashboard is a remote image, so
replacing it also removes the external request. Use a root relative `url('/images/...')` in the
style sheet, because it is served from `/ui/resources/css`. Clear `CustomStylesheet` to fall back to
the dashboard defaults; a configured file that is missing in `wwwroot` is logged as a warning and
also falls back to the defaults.

## Home page

The root of the service (`/`) is a Razor Page (`src/DotNet.ServiceName.Api/Pages/Index.cshtml`) whose
links are built from the configuration of the running environment, so a production deployment does
not advertise UIs it does not expose. Its content is configured in `HomePageOptions`:

```json
"HomePageOptions": {
  "Enabled": true,
  "Title": "Simple Service API",
  "Description": "Template service with API Key authorization, health checks and OpenTelemetry",
  "ShowEnvironment": true,
  "ShowDocumentation": true,
  "ShowHealthChecks": true,
  "Links": []
}
```

| Setting | Effect |
|---|---|
| `Enabled` | Serve the page on `/`. When `false`, Razor Pages are not registered at all and `/` returns `404` |
| `Title`, `Description` | Heading and lead text of the page |
| `ShowEnvironment` | Show the name of the current environment as a badge |
| `ShowDocumentation` | Show Swagger, Scalar and Facet Dashboard links - but only when `SwaggerEnabled` is `true` for the environment. When they are expected and turned off, the page says that the documentation is not available |
| `ShowHealthChecks` | Show the health status link, plus the dashboard link when the Health Checks UI is enabled |
| `Links` | Extra links (title, description, URL, icon, enabled, open in new tab) for anything the configuration above does not cover |
| `DefaultTheme` | Theme the page starts with: `System` (default), `Light` or `Dark` |

The page is a slim top bar (service name, environment badge, theme switch) above the link cards -
the name is the only heading of the document, so the page stays a single h1.

The theme switch has three positions (`System`, `Light`, `Dark`): the choice is stored in the browser
and applied by [`wwwroot/js/theme.js`](src/DotNet.ServiceName.Api/wwwroot/js/theme.js) before the
first paint, so the page never flashes in the wrong theme. The switch is a radio group, so it works
with the keyboard and screen readers, and while `System` is selected the page follows the operating
system when its theme changes. `DefaultTheme` only sets where the switch starts.

Turn the whole page off with `Enabled`, and tune the content per environment - the shipped
[`appsettings.Production.json`](src/DotNet.ServiceName.Api/appsettings.Production.json) is the sample
of a production deployment: `SwaggerEnabled: false` (so no Swagger, Scalar or Facet links), no
environment badge and a link to an internal runbook. Styles live in
[`wwwroot/css/home.css`](src/DotNet.ServiceName.Api/wwwroot/css/home.css) and the icons in
[`wwwroot/icons`](src/DotNet.ServiceName.Api/wwwroot/icons).

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

## Telemetry (OpenTelemetry)

Traces and metrics are collected with [OpenTelemetry](https://opentelemetry.io/) and exported
over OTLP. When `OtlpEndpoint` is empty, the standard `OTEL_EXPORTER_OTLP_*` environment
variables are honored (default endpoint: `http://localhost:4317`):

```json
"TelemetryOptions": {
  "Enabled": true,
  "ServiceName": "dotnet-servicename",
  "ConsoleExporter": false
}
```

Instrumented out of the box: incoming ASP.NET Core requests, outgoing `HttpClient` calls, and
runtime metrics (GC, threads, memory). Set `ConsoleExporter` to `true` to print telemetry
locally without a collector. Every Serilog request entry also carries the `TraceId`, so logs
can be correlated with the corresponding trace.

The registration lives in the `DotNet.ServiceName.ServiceDefaults` project (see
[Aspire](#aspire-local-orchestration)) - `Program.cs` only calls `builder.AddServiceDefaults()`.

## Aspire (local orchestration)

The template ships an Aspire [AppHost](https://aspire.dev/get-started/app-host/)
(`DotNet.ServiceName.AppHost`) that orchestrates the API and a Seq container for local
development, with the Aspire dashboard for structured logs, traces and metrics:

```bash
# install the Aspire CLI once (https://aspire.dev/get-started/install-cli/)
curl -sSL https://aspire.dev/install.sh | bash

# run the AppHost from the repository root
aspire run --project src/DotNet.ServiceName.AppHost/DotNet.ServiceName.AppHost.csproj
```

The dashboard URL is printed by the CLI. What is wired:

* the `seq` container is pinned to host port `5341`, so the Seq sink configured in
  `appsettings.Local.json` keeps working unchanged, and the API waits for it to start
* the AppHost injects the dashboard OTLP endpoint into the API process
  (`OTEL_EXPORTER_OTLP_ENDPOINT`), so traces and metrics flow into the dashboard without any
  configuration - the service defaults exporter already honors those environment variables
* Serilog logs are forwarded to the same endpoint as OTLP logs while that variable is present

The `DotNet.ServiceName.ServiceDefaults` project carries the cross-service plumbing every
service of the solution gets: the OpenTelemetry registration driven by `TelemetryOptions`, a
liveness health check, service discovery, and a standard resilience pipeline for outbound
`HttpClient` calls. Unlike the stock Aspire service defaults it does not map its own health
endpoints, because the template maps `/health`, `/health/live` and `/health/ready` itself.

`docker-compose.yml` remains the standalone option (`docker compose up --build`) - it runs the
same API and Seq pair without Aspire.

## Running behind a proxy / load balancer

Forwarded headers are processed, but only a loopback proxy is trusted by default so clients
cannot spoof `X-Forwarded-*`. When running behind a reverse proxy or load balancer, list its
IP address in configuration:

```json
"ForwardedHeaders": {
  "KnownProxies": ["10.0.0.5"]
}
```

## Tests

Two xUnit projects cover the solution (40 tests in total):

* `DotNet.ServiceName.Application.Tests` - unit tests for services, DTO mapping and DI
  registration (NSubstitute for mocks).
* `DotNet.ServiceName.Api.Tests` - integration tests that boot the whole API in-process with
  `WebApplicationFactory`: API Key authentication (401/403/200), ProblemDetails responses,
  CORS policy (preflight, allowed and disallowed origins), rate limiting (429 with
  ProblemDetails, health endpoints exempt), health check endpoints, OpenAPI document, and
  telemetry wiring.

```bash
# run everything
dotnet test DotNet.ServiceName.sln

# run a single project or filter by name
dotnet test tests/DotNet.ServiceName.Api.Tests
dotnet test --filter "FullyQualifiedName~RateLimiting"
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

# run with Aspire orchestration (API + Seq + dashboard)
aspire run --project src/DotNet.ServiceName.AppHost/DotNet.ServiceName.AppHost.csproj

# call a secured endpoint (default local key)
curl -H "X-API-Key: local-dev-api-key" http://localhost:5050/api/v1/values
```

## Links

1. [Download .NET](https://dotnet.microsoft.com/en-us/download)
