FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app

# For now, we will establish HTTPS via a reverse proxy (e.g. Caddy, NGINX, etc.), so we don't need
# to expose the port 443
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TGB.AccountBE.API/TGB.AccountBE.API.csproj", "TGB.AccountBE.API/"]
RUN dotnet restore "TGB.AccountBE.API/TGB.AccountBE.API.csproj"
COPY . .
WORKDIR "/src/TGB.AccountBE.API"
RUN dotnet build "TGB.AccountBE.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TGB.AccountBE.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TGB.AccountBE.API.dll"]
