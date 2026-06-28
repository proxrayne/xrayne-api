# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS node-build
WORKDIR /src

COPY Directory.Build.props global.json nuget.config XRayne.sln ./
COPY XRayne.Node/XRayne.Node.csproj XRayne.Node/

RUN dotnet restore XRayne.Node/XRayne.Node.csproj

COPY XRayne.Node/ XRayne.Node/

RUN dotnet publish XRayne.Node/XRayne.Node.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true

RUN mkdir -p /app/logs

COPY --from=node-build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "XRayne.Node.dll"]
