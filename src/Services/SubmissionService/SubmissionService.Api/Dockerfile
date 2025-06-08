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

# Copy SubmissionService projects
COPY ["src/Services/SubmissionService/SubmissionService.Api/SubmissionService.Api.csproj", "src/Services/SubmissionService/SubmissionService.Api/"]
COPY ["src/Services/SubmissionService/SubmissionService.Application/SubmissionService.Application.csproj", "src/Services/SubmissionService/SubmissionService.Application/"]
COPY ["src/Services/SubmissionService/SubmissionService.Domain/SubmissionService.Domain.csproj", "src/Services/SubmissionService/SubmissionService.Domain/"]
COPY ["src/Services/SubmissionService/SubmissionService.Infrastructure/SubmissionService.Infrastructure.csproj", "src/Services/SubmissionService/SubmissionService.Infrastructure/"]

RUN dotnet restore "src/Services/SubmissionService/SubmissionService.Api/SubmissionService.Api.csproj"

COPY . .
WORKDIR "/src/src/Services/SubmissionService/SubmissionService.Api"
RUN dotnet build "SubmissionService.Api.csproj" -c Release -o /app/build
RUN dotnet publish "SubmissionService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SubmissionService.Api.dll"]