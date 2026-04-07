using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using Whyce.Platform.Api.Extensions;
using Whyce.Platform.Api.Health;
using Whyce.Platform.Host.Health;
using Whyce.Shared.Contracts.Infrastructure.Health;

namespace Whyce.Platform.Host.Composition.Observability;

/// <summary>
/// Health checks (one per infra dependency), health aggregator, swagger,
/// and controller discovery. Order of IHealthCheck registrations is preserved
/// to match the prior Program.cs ordering used by HealthAggregator.
/// </summary>
public static class ObservabilityComposition
{
    public static IServiceCollection AddObservabilityComposition(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var postgresEventStoreCs = configuration.GetValue<string>("Postgres__ConnectionString")
            ?? throw new InvalidOperationException("Postgres__ConnectionString is required. No fallback.");
        var redisConnectionString = configuration.GetValue<string>("Redis__ConnectionString")
            ?? throw new InvalidOperationException("Redis__ConnectionString is required. No fallback.");
        var kafkaBootstrapServers = configuration.GetValue<string>("Kafka__BootstrapServers")
            ?? throw new InvalidOperationException("Kafka__BootstrapServers is required. No fallback.");
        var opaEndpoint = configuration.GetValue<string>("OPA__Endpoint")
            ?? throw new InvalidOperationException("OPA__Endpoint is required. No fallback.");

        services.AddSingleton<IHealthCheck>(_ =>
            new PostgreSqlHealthCheck(postgresEventStoreCs));

        services.AddSingleton<IHealthCheck>(_ =>
            new KafkaHealthCheck(kafkaBootstrapServers));

        services.AddSingleton<IHealthCheck>(_ =>
            new RedisHealthCheck(redisConnectionString));

        services.AddSingleton<IHealthCheck>(_ =>
        {
            var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            return new OpaHealthCheck(httpClient, opaEndpoint);
        });

        services.AddSingleton<IHealthCheck>(_ =>
        {
            var endpoint = configuration.GetValue<string>("MinIO__Endpoint")
                ?? throw new InvalidOperationException("MinIO__Endpoint is required. No fallback.");
            var accessKey = configuration.GetValue<string>("MinIO__AccessKey")
                ?? throw new InvalidOperationException("MinIO__AccessKey is required. No fallback.");
            var secretKey = configuration.GetValue<string>("MinIO__SecretKey")
                ?? throw new InvalidOperationException("MinIO__SecretKey is required. No fallback.");
            var useSsl = configuration.GetValue<bool>("MinIO__UseSsl", false);

            var client = new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(accessKey, secretKey)
                .WithSSL(useSsl)
                .Build();

            return new MinioHealthCheck(client);
        });

        services.AddSingleton<IHealthCheck>(sp => new RuntimeHealthCheck(sp));

        services.AddSingleton<HealthAggregator>();
        services.AddWhyceSwagger();
        services.AddControllers()
            .AddApplicationPart(typeof(Whyce.Platform.Api.Controllers.HealthController).Assembly);

        return services;
    }
}
