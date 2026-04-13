using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using StackExchange.Redis;
using Whycespace.Platform.Api.Extensions;
using Whycespace.Platform.Api.Health;
using Whycespace.Platform.Host.Health;
using Whycespace.Shared.Contracts.Infrastructure.Health;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Composition.Observability;

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
        // phase1.6-CFG-K1: Section:Key form (see InfrastructureComposition.cs)
        var postgresEventStoreCs = configuration.GetValue<string>("Postgres:ConnectionString")
            ?? throw new InvalidOperationException("Postgres:ConnectionString is required. No fallback.");
        var redisConnectionString = configuration.GetValue<string>("Redis:ConnectionString")
            ?? throw new InvalidOperationException("Redis:ConnectionString is required. No fallback.");
        var kafkaBootstrapServers = configuration.GetValue<string>("Kafka:BootstrapServers")
            ?? throw new InvalidOperationException("Kafka:BootstrapServers is required. No fallback.");
        var opaEndpoint = configuration.GetValue<string>("OPA:Endpoint")
            ?? throw new InvalidOperationException("OPA:Endpoint is required. No fallback.");

        // phase1.5-S5.2.4 / HC-6 (POSTGRES-POOL-HEALTH-01): the
        // postgres health check no longer opens a connection or
        // issues SELECT 1. It evaluates the in-process pool snapshot
        // produced by IPostgresPoolSnapshotProvider via the canonical
        // PostgresPoolHealthEvaluator rule. The previous connection
        // string parameter is no longer required.
        services.AddSingleton<IPostgresPoolSnapshotProvider>(sp =>
            new PostgresPoolSnapshotProvider(
                sp.GetRequiredService<PostgresPoolCatalog>(),
                sp.GetRequiredService<IClock>()));
        services.AddSingleton<IHealthCheck>(sp =>
            new PostgreSqlHealthCheck(sp.GetRequiredService<IPostgresPoolSnapshotProvider>()));

        services.AddSingleton<IHealthCheck>(_ =>
            new KafkaHealthCheck(kafkaBootstrapServers));

        // phase1.5-S5.2.4 / HC-9 (REDIS-HEALTH-01): the Redis
        // health check no longer constructs a per-call multiplexer.
        // It consumes the host's singleton IConnectionMultiplexer
        // (already registered by InfrastructureComposition for
        // MI-1 / outbox / projection paths) and a single
        // RedisHealthOptions singleton. Lightweight Ping-only
        // probe — no set/get round trip on the /Health hot path.
        services.AddSingleton(new RedisHealthOptions());
        services.AddSingleton<IHealthCheck>(sp =>
            new RedisHealthCheck(
                sp.GetRequiredService<IConnectionMultiplexer>(),
                sp.GetRequiredService<RedisHealthOptions>()));
        services.AddSingleton(sp =>
            new RedisHealthSnapshotProvider(
                sp.GetRequiredService<IConnectionMultiplexer>(),
                sp.GetRequiredService<IClock>()));

        services.AddSingleton<IHealthCheck>(_ =>
        {
            var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            return new OpaHealthCheck(httpClient, opaEndpoint);
        });

        services.AddSingleton<IHealthCheck>(_ =>
        {
            var endpoint = configuration.GetValue<string>("MinIO:Endpoint")
                ?? throw new InvalidOperationException("MinIO:Endpoint is required. No fallback.");
            var accessKey = configuration.GetValue<string>("MinIO:AccessKey")
                ?? throw new InvalidOperationException("MinIO:AccessKey is required. No fallback.");
            var secretKey = configuration.GetValue<string>("MinIO:SecretKey")
                ?? throw new InvalidOperationException("MinIO:SecretKey is required. No fallback.");
            var useSsl = configuration.GetValue<bool>("MinIO:UseSsl", false);

            var client = new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(accessKey, secretKey)
                .WithSSL(useSsl)
                .Build();

            return new MinioHealthCheck(client);
        });

        services.AddSingleton<IHealthCheck>(sp => new RuntimeHealthCheck(sp));

        // phase1.5-S5.2.4 / HC-5 (WORKER-LIVENESS-01): worker
        // liveness registry + declared silence ceiling + synthetic
        // "workers" health check. The registry is the storage seam
        // that the three BackgroundService workers (OutboxDepthSampler,
        // KafkaOutboxPublisher, GenericKafkaProjectionConsumerWorker)
        // write to via RecordSuccess after every successful loop
        // iteration. WorkersHealthCheck reads it at probe time and
        // contributes to the canonical RuntimeStateAggregator rule
        // through the standard IHealthCheck fan-out.
        var workerHealthDefaults = new WorkerHealthOptions();
        var workerHealthOptions = new WorkerHealthOptions
        {
            MaxSilenceSeconds = configuration.GetValue<int?>("WorkerHealth:MaxSilenceSeconds")
                ?? workerHealthDefaults.MaxSilenceSeconds,
        };
        services.AddSingleton(workerHealthOptions);
        services.AddSingleton<WorkerLivenessRegistry>();
        services.AddSingleton<IWorkerLivenessRegistry>(sp => sp.GetRequiredService<WorkerLivenessRegistry>());
        services.AddSingleton<IHealthCheck>(sp => new WorkersHealthCheck(
            sp.GetRequiredService<IWorkerLivenessRegistry>(),
            sp.GetRequiredService<WorkerHealthOptions>(),
            sp.GetRequiredService<Whycespace.Shared.Kernel.Domain.IClock>()));

        // phase1.5-S5.2.4 / HC-2 (RUNTIME-STATE-AGGREGATION-01):
        // canonical runtime-state aggregator. Owns the rule that
        // pre-HC-2 lived in HealthAggregator. Depends on the concrete
        // OpaPolicyEvaluator + WhyceChainPostgresAdapter singletons
        // (registered in InfrastructureComposition) so it can read
        // the new side-effect-free IsBreakerOpen getters; the
        // existing IPolicyEvaluator / IChainAnchor interface
        // registrations forward to the same singletons so all other
        // consumers continue to use the interfaces unchanged.
        services.AddSingleton<RuntimeStateAggregator>();
        services.AddSingleton<IRuntimeStateAggregator>(sp => sp.GetRequiredService<RuntimeStateAggregator>());
        // phase1.5-S5.2.4 / HC-8 (MAINTENANCE-MODE-ENFORCEMENT-01):
        // singleton in-process maintenance posture. Default state
        // is "not in maintenance"; declared operator action calls
        // Enter() / Exit() to flip the flag. The runtime control
        // plane consults the IRuntimeMaintenanceModeProvider
        // contract via DI; the host owns the only implementation.
        services.AddSingleton<RuntimeMaintenanceModeProvider>();
        services.AddSingleton<IRuntimeMaintenanceModeProvider>(sp => sp.GetRequiredService<RuntimeMaintenanceModeProvider>());
        services.AddSingleton<HealthAggregator>();
        services.AddWhyceSwagger();
        services.AddControllers()
            .AddApplicationPart(typeof(Whycespace.Platform.Api.Controllers.Platform.Infrastructure.Health.HealthController).Assembly);

        return services;
    }
}
