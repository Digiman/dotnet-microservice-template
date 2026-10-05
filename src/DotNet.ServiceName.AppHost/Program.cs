// Aspire AppHost - the local development orchestrator of the solution.
//
// Running it with `aspire run` starts every resource below, opens the Aspire dashboard
// (structured logs, traces and metrics) and keeps them under one lifecycle: stopping the
// AppHost stops the resources. The API and Seq run exactly as they do in production code
// paths - the AppHost only wires environment and endpoints around them.
using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Seq is the Serilog sink of the service (see the Serilog section of appsettings.Local.json).
//
// - the host port is pinned to 5341, so the sink url "http://localhost:5341" keeps working
//   without per-run changes (Aspire would otherwise assign a random port)
// - the image follows the latest published Seq release - the integration ships with an older
//   default (2025.2). Note that docker reuses cached images: run
//   `docker pull datalust/seq:latest` to bring the local copy up to date
// - ACCEPT_EULA=Y is required, Seq refuses to start without it
// - SEQ_FIRSTRUN_NOAUTHENTICATION skips the first-run admin setup, matching docker-compose
// - the session lifetime recreates the container on every AppHost run, so each run starts
//   with an empty log store instead of accumulating state on disk
var seq = builder.AddSeq("seq", port: 5341)
    .WithImageTag("latest")
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("SEQ_FIRSTRUN_NOAUTHENTICATION", "true")
    .WithLifetime(ContainerLifetime.Session)
    .ExcludeFromManifest();

// The API project runs with its "aspire" launch profile (Local environment, so the Local
// appsettings with the Seq sink apply).
//
// - Aspire injects the dashboard OTLP endpoint into the process (OTEL_EXPORTER_OTLP_ENDPOINT),
//   which is all the OpenTelemetry registration in DotNet.ServiceName.ServiceDefaults needs to
//   export traces and metrics to the dashboard - no configuration required
// - the same endpoint is picked up by Program.cs to forward Serilog logs to the dashboard
// - WaitFor(seq) starts the API after Seq is healthy, so the log sink is reachable from the
//   very first request
var api = builder.AddProject<Projects.DotNet_ServiceName_Api>("api", launchProfileName: "aspire")
    .WithExternalHttpEndpoints()
    .WaitFor(seq);

await builder.Build().RunAsync();