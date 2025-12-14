FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/Services/FinanceService/FinanceService.API/FinanceService.API.csproj", "src/Services/FinanceService/FinanceService.API/"]
COPY ["src/Services/FinanceService/FinanceService.Application/FinanceService.Application.csproj", "src/Services/FinanceService/FinanceService.Application/"]
COPY ["src/Services/FinanceService/FinanceService.Domain/FinanceService.Domain.csproj", "src/Services/FinanceService/FinanceService.Domain/"]
COPY ["src/Services/FinanceService/FinanceService.Infrastructure/FinanceService.Infrastructure.csproj", "src/Services/FinanceService/FinanceService.Infrastructure/"]
COPY ["src/Shared/Shared.Contracts/Shared.Contracts.csproj", "src/Shared/Shared.Contracts/"]
COPY ["src/Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "src/Shared/Shared.Infrastructure/"]
COPY ["src/Shared/Shared.ErrorHandling/ErrorHandling.csproj", "src/Shared/Shared.ErrorHandling/"]
COPY ["src/Shared/Shared.Logging/Logging.csproj", "src/Shared/Shared.Logging/"]
COPY ["src/Shared/Shared.Swagger/Shared.Swagger.csproj", "src/Shared/Shared.Swagger/"]

# Restore dependencies
RUN dotnet restore "src/Services/FinanceService/FinanceService.API/FinanceService.API.csproj"

# Copy source code
COPY . .

# Build
WORKDIR "/src/src/Services/FinanceService/FinanceService.API"
RUN dotnet build "FinanceService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FinanceService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FinanceService.API.dll"]
