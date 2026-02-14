# Stage 1: Build Angular frontend
FROM node:24-alpine AS frontend-build
WORKDIR /app/ClientApp

# Copy package files and install dependencies
COPY src/FlatRate.Web/ClientApp/package*.json ./
RUN npm ci

# Copy source and build
COPY src/FlatRate.Web/ClientApp/ ./
RUN npm run build

# Stage 2: Build .NET application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src

# Copy project files for restore
COPY src/FlatRate.Domain/*.csproj ./FlatRate.Domain/
COPY src/FlatRate.Application/*.csproj ./FlatRate.Application/
COPY src/FlatRate.Infrastructure/*.csproj ./FlatRate.Infrastructure/
COPY src/FlatRate.Web/*.csproj ./FlatRate.Web/

# Restore dependencies (Web project pulls in all dependencies)
RUN dotnet restore FlatRate.Web/FlatRate.Web.csproj

# Copy all source code
COPY src/FlatRate.Domain/ ./FlatRate.Domain/
COPY src/FlatRate.Application/ ./FlatRate.Application/
COPY src/FlatRate.Infrastructure/ ./FlatRate.Infrastructure/
COPY src/FlatRate.Web/ ./FlatRate.Web/

# Copy built Angular app to wwwroot
COPY --from=frontend-build /app/wwwroot ./FlatRate.Web/wwwroot/

# Build and publish
RUN dotnet publish FlatRate.Web/FlatRate.Web.csproj -c Release -o /app/publish --no-restore

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install Kerberos library required by Npgsql for some auth modes
RUN apt-get update && apt-get install -y --no-install-recommends libkrb5-3 && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN useradd --create-home --shell /bin/bash appuser || adduser -D -s /bin/sh appuser

# Copy published app
COPY --from=backend-build /app/publish .

# Set ownership
RUN chown -R appuser:appuser /app
USER appuser

# Cloud Run uses PORT environment variable
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "FlatRate.Web.dll"]
