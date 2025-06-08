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

# Copy ExamService projects
COPY ["src/Services/ExamService/ExamService.Api/ExamService.Api.csproj", "src/Services/ExamService/ExamService.Api/"]
COPY ["src/Services/ExamService/ExamService.Application/ExamService.Application.csproj", "src/Services/ExamService/ExamService.Application/"]
COPY ["src/Services/ExamService/ExamService.Domain/ExamService.Domain.csproj", "src/Services/ExamService/ExamService.Domain/"]
COPY ["src/Services/ExamService/ExamService.Infrastructure/ExamService.Infrastructure.csproj", "src/Services/ExamService/ExamService.Infrastructure/"]

RUN dotnet restore "src/Services/ExamService/ExamService.Api/ExamService.Api.csproj"

COPY . .
WORKDIR "/src/src/Services/ExamService/ExamService.Api"
RUN dotnet build "ExamService.Api.csproj" -c Release -o /app/build
RUN dotnet publish "ExamService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ExamService.Api.dll"]