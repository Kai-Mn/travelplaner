# ──────────────────────────────────────────────────────────────
# Stage 1: Build Angular SPA
# ──────────────────────────────────────────────────────────────
FROM node:24-alpine AS frontend-build
WORKDIR /app/frontend
COPY frontend/package*.json ./
RUN npm ci --quiet
COPY frontend/ .
RUN npx ng build --configuration production

# ──────────────────────────────────────────────────────────────
# Stage 2: Build .NET API
# ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build
WORKDIR /app/backend
COPY backend/ .
RUN dotnet restore TravelPlaner.sln
RUN dotnet publish src/TravelPlaner.Api/TravelPlaner.Api.csproj \
    -c Release \
    -o /publish \
    --no-restore

# ──────────────────────────────────────────────────────────────
# Stage 3: Runtime image
# ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy API
COPY --from=backend-build /publish .

# Copy Angular build output into wwwroot so .NET serves the SPA
COPY --from=frontend-build /app/frontend/dist/frontend/browser ./wwwroot

# Create data directories
RUN mkdir -p /data/images

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV PERSISTENCE_PROVIDER=sqlite
ENV SQLITE__PATH=/data/travelplanner.db
ENV IMAGE_STORE_PATH=/data/images

EXPOSE 8080

ENTRYPOINT ["dotnet", "TravelPlaner.Api.dll"]
