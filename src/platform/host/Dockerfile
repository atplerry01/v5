# Runtime-only Dockerfile. The dotnet publish is performed on the host (where
# NuGet connectivity works) and the published output is copied into a minimal
# aspnet:10.0 runtime image. This avoids issuing `dotnet restore` from inside
# a build container that may not have access to api.nuget.org (corporate
# network, offline builds, etc.).
#
# Expected publish path (relative to the build context, which is the repo
# root per docker-compose.validation.yml): artifacts/host-publish/
#
# Classification: phase5-operational-activation / economic-system

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY artifacts/host-publish/ .
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "Whycespace.Host.dll"]
