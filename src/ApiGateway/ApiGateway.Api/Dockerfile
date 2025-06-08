FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/ApiGateway/ApiGateway.Api/ApiGateway.Api.csproj", "src/ApiGateway/ApiGateway.Api/"]
RUN dotnet restore "src/ApiGateway/ApiGateway.Api/ApiGateway.Api.csproj"

COPY . .
WORKDIR "/src/src/ApiGateway/ApiGateway.Api"
RUN dotnet build "ApiGateway.Api.csproj" -c Release -o /app/build
RUN dotnet publish "ApiGateway.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ApiGateway.Api.dll"]