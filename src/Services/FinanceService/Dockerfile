FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5005

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Services/FinanceService/FinanceService.API/FinanceService.API.csproj", "Services/FinanceService/FinanceService.API/"]
COPY ["src/Services/FinanceService/FinanceService.Infrastructure/FinanceService.Infrastructure.csproj", "Services/FinanceService/FinanceService.Infrastructure/"]
COPY ["src/Services/FinanceService/FinanceService.Application/FinanceService.Application.csproj", "Services/FinanceService/FinanceService.Application/"]
COPY ["src/Services/FinanceService/FinanceService.Domain/FinanceService.Domain.csproj", "Services/FinanceService/FinanceService.Domain/"]
COPY ["src/Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]
COPY ["src/Shared/Shared.Contracts/Shared.Contracts.csproj", "Shared/Shared.Contracts/"]
RUN dotnet restore "Services/FinanceService/FinanceService.API/FinanceService.API.csproj"
COPY src/ .
WORKDIR "/src/Services/FinanceService/FinanceService.API"
RUN dotnet build "FinanceService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FinanceService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FinanceService.API.dll"]
