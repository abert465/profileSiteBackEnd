# Single-container build: the Vite SPA is compiled first, then copied into the
# published ASP.NET app's wwwroot so both are served from one origin.

# ---- stage 1: build the React frontend ----
FROM node:22-alpine AS web
WORKDIR /src/web

# Copy manifests first so npm ci is cached until dependencies actually change.
COPY web/package.json web/package-lock.json ./
RUN npm ci

COPY web/ ./
RUN npm run build

# ---- stage 2: build and publish the API ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore against the project file alone, for the same caching reason.
# The test project is deliberately excluded from the image build.
COPY profileSiteBackEnd/profileSiteBackEnd.csproj profileSiteBackEnd/
RUN dotnet restore profileSiteBackEnd/profileSiteBackEnd.csproj

COPY profileSiteBackEnd/ profileSiteBackEnd/
RUN dotnet publish profileSiteBackEnd/profileSiteBackEnd.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

# ---- stage 3: runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish ./
COPY --from=web /src/web/dist ./wwwroot

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "profileSiteBackEnd.dll"]
