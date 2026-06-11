# ---- Build Stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and project files first (for Docker layer caching)
COPY backend/RagService.sln ./backend/
COPY backend/src/RagService.Api/RagService.Api.csproj ./backend/src/RagService.Api/
COPY backend/src/RagService.Core/RagService.Core.csproj ./backend/src/RagService.Core/
COPY backend/src/RagService.Infrastructure/RagService.Infrastructure.csproj ./backend/src/RagService.Infrastructure/

# Restore NuGet packages
RUN dotnet restore backend/RagService.sln

# Copy all source code
COPY backend/ ./backend/

# Publish in Release mode
RUN dotnet publish backend/src/RagService.Api/RagService.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ---- Runtime Stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Copy docs folder (ingested on startup)
COPY backend/docs ./docs

# Render uses port 10000 by default
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "RagService.Api.dll"]
