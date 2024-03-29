#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/DotNet.ServiceName.Api/DotNet.ServiceName.Api.csproj", "src/DotNet.ServiceName.Api/"]
RUN dotnet restore "src/DotNet.ServiceName.Api/DotNet.ServiceName.Api.csproj"
COPY . .
WORKDIR "/src/src/DotNet.ServiceName.Api"
RUN dotnet build "DotNet.ServiceName.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DotNet.ServiceName.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DotNet.ServiceName.Api.dll"]
