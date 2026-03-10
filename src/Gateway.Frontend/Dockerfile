FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
# Usuwamy stąd USER $APP_UID, żeby nie psuło nam uprawnień w finalnym obrazie
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Gateway.Frontend/Frontend.Gateway.csproj", "src/Gateway.Frontend/"]
RUN dotnet restore "src/Gateway.Frontend/Frontend.Gateway.csproj"
COPY . .
WORKDIR "/src/src/Gateway.Frontend"
RUN dotnet build "./Frontend.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Frontend.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
USER root
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Frontend.Gateway.dll"]