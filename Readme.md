# Simple Service Template for .NET application

TODO: add here information and details about the template + diagrams

[![Lint Code Base](https://github.com/Digiman/dotnet-microservice-template/actions/workflows/super-linter.yml/badge.svg)](https://github.com/Digiman/dotnet-microservice-template/actions/workflows/super-linter.yml)
[![CodeQL](https://github.com/Digiman/dotnet-microservice-template/actions/workflows/codeql.yml/badge.svg)](https://github.com/Digiman/dotnet-microservice-template/actions/workflows/codeql.yml)

## Introduction

TBD

## Tech stack

Application developed and used next technologies (on the backend) and components:

* .NET 7 (STS)
* [Serilog](https://github.com/serilog/serilog) for logging
* [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) for Swagger (OpenAPI)
* HealthCheck UI for ASP.NET Core - [Enterprise HealthChecks for ASP.NET Core Diagnostics Package](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks)

## Logging

Service/web application use Serilog to write and generate structure logs with details how application working. It's possible to configure logs to send to the different services like Splunk to monitor in one single place or use other tools to read the logs. Depending on hosting type and where the service wil be placed.

## Monitoring

No any monitoring tools/services are available in the service at this time.

## Availability and Health check

TBD

## Build Process for Local Development

* You have Docker installed - ideally latest version of the tool.
* You have .NET 7 installed (SDK and runtime).
* Visual Studio 2022 (17.5+) or JetBrains Rider (2023.1+) or Visual Studio Code as IDE - one of them, better for you, all them is appropriate.

Optionally:
* [Tye](https://github.com/dotnet/tye) - Project Tye for local development and helper for containers and Kubernetes.

## Links

1. [Download .NET](https://dotnet.microsoft.com/en-us/download)
