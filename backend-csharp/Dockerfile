# Build stage
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY PdfToOfd.Api/PdfToOfd.Api.csproj PdfToOfd.Api/
RUN dotnet restore PdfToOfd.Api/PdfToOfd.Api.csproj

COPY . .
WORKDIR /src/PdfToOfd.Api
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Install curl for healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create data directory
RUN mkdir -p /data/pdf /data/ofd

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:5000/api/health || exit 1

ENTRYPOINT ["dotnet", "PdfToOfd.Api.dll"]
