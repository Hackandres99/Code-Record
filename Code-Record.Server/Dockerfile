# Fase de base para la aplicación ASP.NET
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
# Puerto para SQL Server
EXPOSE 1433

# Fase para compilar el proyecto de servicio
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS with-node
RUN apt-get update \
    && apt-get install -y curl gnupg2 \
    && curl -sL https://deb.nodesource.com/setup_20.x | bash \
    && apt-get -y install nodejs \
    && npm install -g @angular/cli \
    && apt-get clean

# Fase para construir la aplicación
FROM with-node AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Code-Record.Server/Code-Record.Server.csproj", "Code-Record.Server/"]
COPY ["code-record.client/code-record.client.esproj", "code-record.client/"]
RUN dotnet restore "./Code-Record.Server/Code-Record.Server.csproj"
COPY . . 
WORKDIR "/src/Code-Record.Server"
RUN dotnet build "./Code-Record.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Fase para publicar la aplicación
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Code-Record.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Fase final para ejecutar la aplicación
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Code-Record.Server.dll"]