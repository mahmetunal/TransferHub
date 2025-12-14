FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["src/Shared/Shared.Common/Shared.Common.csproj", "src/Shared/Shared.Common/"]
COPY ["src/Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "src/Shared/Shared.Infrastructure/"]
COPY ["src/Services/MoneyTransfer/MoneyTransfer.Domain/MoneyTransfer.Domain.csproj", "src/Services/MoneyTransfer/MoneyTransfer.Domain/"]
COPY ["src/Services/MoneyTransfer/MoneyTransfer.Application/MoneyTransfer.Application.csproj", "src/Services/MoneyTransfer/MoneyTransfer.Application/"]
COPY ["src/Services/MoneyTransfer/MoneyTransfer.Infrastructure/MoneyTransfer.Infrastructure.csproj", "src/Services/MoneyTransfer/MoneyTransfer.Infrastructure/"]
COPY ["src/Services/MoneyTransfer/MoneyTransfer.API/MoneyTransfer.API.csproj", "src/Services/MoneyTransfer/MoneyTransfer.API/"]

# Restore dependencies
RUN dotnet restore "src/Services/MoneyTransfer/MoneyTransfer.API/MoneyTransfer.API.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/src/Services/MoneyTransfer/MoneyTransfer.API"
RUN dotnet build "MoneyTransfer.API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "MoneyTransfer.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs

ENTRYPOINT ["dotnet", "MoneyTransfer.API.dll"]
