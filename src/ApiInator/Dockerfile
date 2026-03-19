FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/ApiInator/ApiInator.csproj", "src/ApiInator/"]
RUN dotnet restore "src/ApiInator/ApiInator.csproj"

COPY . .

WORKDIR "/src/src/ApiInator"
RUN dotnet publish "ApiInator.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update && \
    apt-get install -y python3.12 python3.12-venv && \
    rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
COPY --from=build /src/src/ApiInator/Infrastructure ./Infrastructure

RUN python3.12 -m venv /app/Infrastructure/venv && \
    /app/Infrastructure/venv/bin/pip install -r /app/Infrastructure/requirements.txt

ENTRYPOINT ["dotnet", "ApiInator.dll"]