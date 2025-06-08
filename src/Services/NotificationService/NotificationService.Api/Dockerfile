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

# Copy NotificationService projects
COPY ["src/Services/NotificationService/NotificationService.Api/NotificationService.Api.csproj", "src/Services/NotificationService/NotificationService.Api/"]
COPY ["src/Services/NotificationService/NotificationService.Application/NotificationService.Application.csproj", "src/Services/NotificationService/NotificationService.Application/"]
COPY ["src/Services/NotificationService/NotificationService.Domain/NotificationService.Domain.csproj", "src/Services/NotificationService/NotificationService.Domain/"]
COPY ["src/Services/NotificationService/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj", "src/Services/NotificationService/NotificationService.Infrastructure/"]

RUN dotnet restore "src/Services/NotificationService/NotificationService.Api/NotificationService.Api.csproj"

COPY . .
WORKDIR "/src/src/Services/NotificationService/NotificationService.Api"
RUN dotnet build "NotificationService.Api.csproj" -c Release -o /app/build
RUN dotnet publish "NotificationService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotificationService.Api.dll"]