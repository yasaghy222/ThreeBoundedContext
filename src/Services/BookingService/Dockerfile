FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/Services/BookingService/BookingService.API/BookingService.API.csproj", "src/Services/BookingService/BookingService.API/"]
COPY ["src/Services/BookingService/BookingService.Application/BookingService.Application.csproj", "src/Services/BookingService/BookingService.Application/"]
COPY ["src/Services/BookingService/BookingService.Domain/BookingService.Domain.csproj", "src/Services/BookingService/BookingService.Domain/"]
COPY ["src/Services/BookingService/BookingService.Infrastructure/BookingService.Infrastructure.csproj", "src/Services/BookingService/BookingService.Infrastructure/"]
COPY ["src/Shared/Shared.Contracts/Shared.Contracts.csproj", "src/Shared/Shared.Contracts/"]
COPY ["src/Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "src/Shared/Shared.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/Services/BookingService/BookingService.API/BookingService.API.csproj"

# Copy source code
COPY . .

# Build
WORKDIR "/src/src/Services/BookingService/BookingService.API"
RUN dotnet build "BookingService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BookingService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookingService.API.dll"]
