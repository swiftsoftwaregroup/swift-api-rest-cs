# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.sln .
COPY swift-api/*.csproj ./swift-api/
COPY swift-api-tests/*.csproj ./swift-api-tests/

RUN dotnet restore

# copy everything else and build app
COPY swift-api/. ./swift-api/
COPY swift-api-tests/. ./swift-api-tests/

WORKDIR /source/swift-api

RUN dotnet publish -c release -o /app

COPY redoc/. /app/redoc/

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app ./

EXPOSE 5000

ENTRYPOINT ["dotnet", "swift-api.dll"]