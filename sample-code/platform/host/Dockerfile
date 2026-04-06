FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files for restore
COPY src/platform/host/Whycespace.FoundationHost.csproj src/platform/host/
COPY src/platform/Whycespace.Platform.csproj src/platform/
COPY src/runtime/Whycespace.Runtime.csproj src/runtime/
COPY src/shared/Whycespace.Shared.csproj src/shared/
COPY src/engines/T0U/Whycespace.Engines.T0U.csproj src/engines/T0U/
COPY src/engines/T2E/Whycespace.Engines.T2E.csproj src/engines/T2E/
COPY src/engines/T4A/Whycespace.Engines.T4A.csproj src/engines/T4A/
COPY src/systems/Whycespace.Systems.csproj src/systems/
COPY infrastructure/adapters/Whycespace.Infrastructure.Adapters.csproj infrastructure/adapters/
COPY src/domain/Whycespace.Domain.csproj src/domain/

RUN dotnet restore "src/platform/host/Whycespace.FoundationHost.csproj"

# Copy all source
COPY src/ src/
COPY infrastructure/adapters/ infrastructure/adapters/

RUN dotnet build "src/platform/host/Whycespace.FoundationHost.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/platform/host/Whycespace.FoundationHost.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Whycespace.FoundationHost.dll"]
