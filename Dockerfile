#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["DotNet.ServiceName.sln", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["src/DotNet.ServiceName.Api/DotNet.ServiceName.Api.csproj", "src/DotNet.ServiceName.Api/"]
COPY ["src/DotNet.ServiceName.Application/DotNet.ServiceName.Application.csproj", "src/DotNet.ServiceName.Application/"]
COPY ["src/DotNet.ServiceName.Common/DotNet.ServiceName.Common.csproj", "src/DotNet.ServiceName.Common/"]
RUN dotnet restore "src/DotNet.ServiceName.Api/DotNet.ServiceName.Api.csproj"
COPY . .
WORKDIR "/src/src/DotNet.ServiceName.Api"
RUN dotnet build "DotNet.ServiceName.Api.csproj" -c Release --no-restore -o /app/build

FROM build AS publish
RUN dotnet publish "DotNet.ServiceName.Api.csproj" -c Release --no-restore -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Runs the container as the unprivileged 'app' user shipped with the .NET images ($APP_UID)
USER 1654

# Probes the /health endpoint without needing curl/wget in the runtime image
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD ["/bin/bash", "-c", "exec 3<>/dev/tcp/localhost/8080 && printf 'GET /health HTTP/1.0\r\n\r\n' >&3 && grep -q '200 OK' <&3"]

ENTRYPOINT ["dotnet", "DotNet.ServiceName.Api.dll"]
