# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS node-build
WORKDIR /src

COPY Directory.Build.props global.json nuget.config sln ./
COPY Contracts/Contracts.csproj Contracts/
COPY Data/Data.csproj Data/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
COPY Api/Api.csproj Api/

RUN dotnet restore Api/Api.csproj

COPY Contracts/ Contracts/
COPY Data/ Data/
COPY Infrastructure/ Infrastructure/
COPY Api/ Api/

RUN dotnet publish Api/Api.csproj \
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
ENTRYPOINT ["dotnet", "Api.dll"]
