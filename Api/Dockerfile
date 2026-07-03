# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS api-build
WORKDIR /src

COPY Directory.Build.props global.json nuget.config XRayne.sln ./
COPY Api/Api.csproj Api/
COPY Contracts/Contracts.csproj Contracts/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
COPY Repositories/Repositories.csproj Repositories/

RUN dotnet restore Api/Api.csproj

COPY . .

RUN dotnet publish Api/Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true

RUN mkdir -p /app/logs /app/shared

COPY --from=api-build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Api.dll"]
