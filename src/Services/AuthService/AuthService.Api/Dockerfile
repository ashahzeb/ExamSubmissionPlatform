FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy shared projects
COPY ["src/Shared/Common.Domain/Common.Domain.csproj", "src/Shared/Common.Domain/"]
COPY ["src/Shared/Common.Application/Common.Application.csproj", "src/Shared/Common.Application/"]
COPY ["src/Shared/Common.Infrastructure/Common.Infrastructure.csproj", "src/Shared/Common.Infrastructure/"]
COPY ["src/Shared/Common.Contracts/Common.Contracts.csproj", "src/Shared/Common.Contracts/"]

# Copy AuthService projects
COPY ["src/Services/AuthService/AuthService.Api/AuthService.Api.csproj", "src/Services/AuthService/AuthService.Api/"]
COPY ["src/Services/AuthService/AuthService.Application/AuthService.Application.csproj", "src/Services/AuthService/AuthService.Application/"]
COPY ["src/Services/AuthService/AuthService.Domain/AuthService.Domain.csproj", "src/Services/AuthService/AuthService.Domain/"]
COPY ["src/Services/AuthService/AuthService.Infrastructure/AuthService.Infrastructure.csproj", "src/Services/AuthService/AuthService.Infrastructure/"]

RUN dotnet restore "src/Services/AuthService/AuthService.Api/AuthService.Api.csproj"

COPY . .
WORKDIR "/src/src/Services/AuthService/AuthService.Api"
RUN dotnet build "AuthService.Api.csproj" -c Release -o /app/build
RUN dotnet publish "AuthService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthService.Api.dll"]