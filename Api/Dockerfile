# syntax=docker/dockerfile:1

FROM node:22-alpine AS ui-build
WORKDIR /src/Dashboard

ARG VITE_API_DOMAIN=
ENV VITE_API_DOMAIN=${VITE_API_DOMAIN}

COPY Dashboard/package.json Dashboard/package-lock.json ./
RUN npm ci

COPY Dashboard/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS api-build
WORKDIR /src

COPY Directory.Build.props global.json nuget.config XRayne.sln ./
COPY Api/Api.csproj Api/
COPY Contracts/Contracts.csproj Contracts/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
COPY Repositories/Repositories.csproj Repositories/

RUN dotnet restore Api/Api.csproj

COPY . .
COPY --from=ui-build /src/Dashboard/build/client /ui-build/client
RUN rm -rf Api/wwwroot && \
    mkdir -p Api/wwwroot && \
    cp -R /ui-build/client/. Api/wwwroot/ && \
    test -f Api/wwwroot/index.html

RUN dotnet publish Api/Api.csproj \
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
ENTRYPOINT ["dotnet", "Api.dll"]
