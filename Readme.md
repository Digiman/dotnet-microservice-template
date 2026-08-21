# Simple Service Template for .NET application

TODO: add here information and details about the template + diagrams

[![CI](https://github.com/Digiman/dotnet-microservice-template/actions/workflows/ci.yml/badge.svg)](https://github.com/Digiman/dotnet-microservice-template/actions/workflows/ci.yml)
[![Lint Code Base](https://github.com/Digiman/dotnet-microservice-template/actions/workflows/super-linter.yml/badge.svg)](https://github.com/Digiman/dotnet-microservice-template/actions/workflows/super-linter.yml)
[![CodeQL](https://github.com/Digiman/dotnet-microservice-template/actions/workflows/codeql.yml/badge.svg)](https://github.com/Digiman/dotnet-microservice-template/actions/workflows/codeql.yml)

## Introduction

TBD

This repository is a template for a .NET microservice. It contains a single Web API project
(`DotNet.ServiceName.Api`) with a layered structure (Application, Common), API versioning,
Swagger/OpenAPI documentation, structured logging, health checks with a dashboard, and a
Docker setup for local development.

## Tech stack

Application developed and used next technologies (on the backend) and components:

* .NET 10 (LTS) - see [`global.json`](global.json) for the pinned SDK version
* [Serilog](https://github.com/serilog/serilog) for logging
* [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) for Swagger (OpenAPI)
* [Asp.Versioning](https://github.com/dotnet/aspnet-api-versioning) for API versioning (URL segment based)
* [Facet](https://github.com/Tim-Maes/Facet) for compile-time generated DTOs and mapping (no runtime reflection)
* HealthCheck UI for ASP.NET Core - [DotNetDiag HealthChecks for ASP.NET Core Diagnostics Package](https://github.com/DotNetDiag/HealthChecks)
* Central Package Management via [`Directory.Packages.props`](Directory.Packages.props)

## Logging

Service/web application use Serilog to write and generate structure logs with details how application working. It's possible to configure logs to send to the different services like Splunk to monitor in one single place or use other tools to read the logs. Depending on hosting type and where the service wil be placed.

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
# then open http://localhost:5050/swagger/index.html
```

## Links

1. [Download .NET](https://dotnet.microsoft.com/en-us/download)
