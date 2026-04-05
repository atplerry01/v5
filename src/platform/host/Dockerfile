FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/platform/host/Whycespace.Host.csproj src/platform/host/
COPY src/shared/ src/shared/
COPY src/infrastructure/ src/infrastructure/
COPY src/runtime/ src/runtime/
COPY src/platform/api/ src/platform/api/

RUN dotnet restore src/platform/host/Whycespace.Host.csproj

COPY src/ src/
RUN dotnet publish src/platform/host/Whycespace.Host.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "Whycespace.Host.dll"]
