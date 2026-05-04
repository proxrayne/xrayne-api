# syntax=docker/dockerfile:1

FROM node:22-alpine AS ui-build
WORKDIR /src/XRayne.UI

ARG VITE_API_DOMAIN=
ENV VITE_API_DOMAIN=${VITE_API_DOMAIN}

COPY XRayne.UI/package.json XRayne.UI/package-lock.json ./
RUN npm ci

COPY XRayne.UI/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS api-build
WORKDIR /src

COPY Directory.Build.props global.json nuget.config XRayne.sln ./
COPY XRayne.Api/XRayne.Api.csproj XRayne.Api/
COPY XRayne.Contracts/XRayne.Contracts.csproj XRayne.Contracts/
COPY XRayne.Core/XRayne.Core.csproj XRayne.Core/
COPY XRayne.Infrastructure/XRayne.Infrastructure.csproj XRayne.Infrastructure/
COPY XRayne.Repositories/XRayne.Repositories.csproj XRayne.Repositories/

RUN dotnet restore XRayne.Api/XRayne.Api.csproj

COPY . .
COPY --from=ui-build /src/XRayne.UI/build/client /ui-build/client
RUN rm -rf XRayne.Api/wwwroot && \
    mkdir -p XRayne.Api/wwwroot && \
    cp -R /ui-build/client/. XRayne.Api/wwwroot/ && \
    test -f XRayne.Api/wwwroot/index.html

RUN dotnet publish XRayne.Api/XRayne.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    -p:UseAppHost=false

RUN test -f /app/publish/wwwroot/index.html

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true

RUN mkdir -p /app/logs /app/shared

COPY --from=api-build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "XRayne.Api.dll"]
