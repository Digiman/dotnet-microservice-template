using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Seq receives the Serilog events of the service (see the Serilog section of
// appsettings.Local.json). The host port is pinned to 5341 so the configured
// sink url http://localhost:5341 works without per-run changes.
var seq = builder.AddSeq("seq", port: 5341)
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithLifetime(ContainerLifetime.Persistent)
    .ExcludeFromManifest();

var api = builder.AddProject<Projects.DotNet_ServiceName_Api>("api", launchProfileName: "aspire")
    .WithExternalHttpEndpoints()
    .WaitFor(seq);

builder.Build().Run();