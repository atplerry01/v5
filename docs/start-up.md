

docker rm -f $(docker ps -aq) && docker compose -f infrastructure/deployment/docker-compose.yml up -d postgres redis opa kafka minio


docker rm -f whyce-kafka whyce-opa && docker compose -f infrastructure/docker-compose.yml up -d kafka opa

dotnet run --project src/platform/host/Whycespace.Host.csproj



docker compose -f infrastructure/deployment/docker-compose.yml up -d postgres postgres-projections redis opa kafka minio whycechain-db 2>&1