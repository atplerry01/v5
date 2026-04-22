using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Adapters;
using Whycespace.Projections.Constitutional.Chain.AnchorRecord;
using Whycespace.Projections.Constitutional.Chain.EvidenceRecord;
using Whycespace.Projections.Constitutional.Chain.Ledger;
using Whycespace.Projections.Shared;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Projection;
using Whycespace.Shared.Contracts.Constitutional.Chain.AnchorRecord;
using Whycespace.Shared.Contracts.Constitutional.Chain.EvidenceRecord;
using Whycespace.Shared.Contracts.Constitutional.Chain.Ledger;
using Whycespace.Shared.Contracts.Infrastructure.Messaging;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Composition.Constitutional.Chain.Projection;

public static class ChainProjectionModule
{
    public static IServiceCollection AddChainProjection(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Projection stores ────────────────────────────────────
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<AnchorRecordReadModel>("projection_constitutional_chain_anchor_record", "anchor_record_read_model", "AnchorRecord"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<EvidenceRecordReadModel>("projection_constitutional_chain_evidence_record", "evidence_record_read_model", "EvidenceRecord"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<LedgerReadModel>("projection_constitutional_chain_ledger", "ledger_read_model", "Ledger"));

        // ── Projection handlers ──────────────────────────────────
        services.AddSingleton(sp => new AnchorRecordProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<AnchorRecordReadModel>>()));
        services.AddSingleton(sp => new EvidenceRecordProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<EvidenceRecordReadModel>>()));
        services.AddSingleton(sp => new LedgerProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<LedgerReadModel>>()));

        // ── Kafka projection consumers ───────────────────────────
        var kafkaBootstrapServers = configuration.GetValue<string>("Kafka:BootstrapServers")
            ?? throw new InvalidOperationException("Kafka:BootstrapServers is required. No fallback.");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.constitutional.chain.anchor-record.events",
            consumerGroup: "whyce.projection.constitutional.chain.anchor-record",
            projectionSchema: "projection_constitutional_chain_anchor_record",
            projectionTable: "anchor_record_read_model",
            aggregateType: "AnchorRecord");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.constitutional.chain.evidence-record.events",
            consumerGroup: "whyce.projection.constitutional.chain.evidence-record",
            projectionSchema: "projection_constitutional_chain_evidence_record",
            projectionTable: "evidence_record_read_model",
            aggregateType: "EvidenceRecord");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.constitutional.chain.ledger.events",
            consumerGroup: "whyce.projection.constitutional.chain.ledger",
            projectionSchema: "projection_constitutional_chain_ledger",
            projectionTable: "ledger_read_model",
            aggregateType: "Ledger");

        return services;
    }

    public static void RegisterProjections(IServiceProvider provider, ProjectionRegistry projection)
    {
        var anchorHandler = provider.GetRequiredService<AnchorRecordProjectionHandler>();
        projection.Register("AnchorRecordCreatedEvent", anchorHandler);
        projection.Register("AnchorRecordSealedEvent", anchorHandler);

        var evidenceHandler = provider.GetRequiredService<EvidenceRecordProjectionHandler>();
        projection.Register("EvidenceRecordCreatedEvent", evidenceHandler);
        projection.Register("EvidenceRecordArchivedEvent", evidenceHandler);

        var ledgerHandler = provider.GetRequiredService<LedgerProjectionHandler>();
        projection.Register("LedgerOpenedEvent", ledgerHandler);
        projection.Register("LedgerSealedEvent", ledgerHandler);
    }

    private static void RegisterWorker(
        IServiceCollection services,
        string kafkaBootstrapServers,
        string topic,
        string consumerGroup,
        string projectionSchema,
        string projectionTable,
        string aggregateType)
    {
        services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(sp =>
            new GenericKafkaProjectionConsumerWorker(
                kafkaBootstrapServers,
                topic,
                consumerGroup,
                sp.GetRequiredService<EventDeserializer>(),
                sp.GetRequiredService<ProjectionRegistry>(),
                new PostgresProjectionWriter(
                    sp.GetRequiredService<ProjectionsDataSource>(),
                    projectionSchema,
                    projectionTable,
                    aggregateType),
                sp.GetRequiredService<IClock>(),
                sp.GetRequiredService<KafkaConsumerOptions>(),
                sp.GetRequiredService<Whycespace.Shared.Contracts.Infrastructure.Health.IWorkerLivenessRegistry>(),
                sp.GetService<Microsoft.Extensions.Logging.ILogger<GenericKafkaProjectionConsumerWorker>>(),
                pollTimeout: null,
                deadLetterStore: sp.GetService<Whycespace.Shared.Contracts.Infrastructure.Messaging.IDeadLetterStore>(),
                kafkaBreaker: sp.GetService<ICircuitBreakerRegistry>()?.TryGet("kafka-producer"),
                topicNameResolver: sp.GetService<TopicNameResolver>(),
                retryOptions: sp.GetService<RetryTierOptions>(),
                randomProvider: sp.GetService<IRandomProvider>()));
    }
}
