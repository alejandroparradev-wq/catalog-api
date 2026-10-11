# Build stage: compila y publica la API
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/CatalogApi/CatalogApi.csproj", "src/CatalogApi/"]
RUN dotnet restore "src/CatalogApi/CatalogApi.csproj" -r linux-x64

COPY src/ src/
# -r linux-x64: incluye los binarios nativos de SQLite para el contenedor
RUN dotnet publish "src/CatalogApi/CatalogApi.csproj" -c Release -o /app/publish \
    /p:UseAppHost=false -r linux-x64 --self-contained false

# Runtime stage: imagen mínima con el runtime de ASP.NET (.NET 10)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080 5443
ENTRYPOINT ["dotnet", "CatalogApi.dll"]