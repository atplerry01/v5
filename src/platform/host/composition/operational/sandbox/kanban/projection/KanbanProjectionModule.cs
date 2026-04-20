using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Adapters;
using Whycespace.Projections.Operational.Sandbox.Kanban.Board;
using Whycespace.Projections.Operational.Sandbox.Kanban.Card;
using Whycespace.Projections.Operational.Sandbox.Kanban.List;
using Whycespace.Projections.Shared;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Projection;
using Whycespace.Shared.Contracts.Operational.Sandbox.Kanban.Board;
using Whycespace.Shared.Kernel.Domain;
using PostgresProjectionWriter = Whycespace.Platform.Host.Adapters.PostgresProjectionWriter;

namespace Whycespace.Platform.Host.Composition.Operational.Sandbox.Kanban.Projection;

/// <summary>
/// Kanban projection module — projection handler DI registrations, store factory,
/// Kafka consumer, and projection registry bindings.
/// </summary>
public static class KanbanProjectionModule
{
    public static IServiceCollection AddKanbanProjection(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Projection layer — factory-created store + domain-focused handlers
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<KanbanBoardReadModel>("projection_operational_sandbox_kanban", "kanban_read_model", "Kanban"));
        services.AddSingleton(sp => new KanbanBoardProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<KanbanBoardReadModel>>()));
        services.AddSingleton(sp => new KanbanListProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<KanbanBoardReadModel>>()));
        services.AddSingleton(sp => new KanbanCardProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<KanbanBoardReadModel>>()));

        // Kafka projection consumer
        var kafkaBootstrapServers = configuration.GetValue<string>("Kafka:BootstrapServers")
            ?? throw new InvalidOperationException("Kafka:BootstrapServers is required. No fallback.");

        const string topic = "whyce.operational.sandbox.kanban.events";
        const string consumerGroup = "whyce.projection.operational.sandbox.kanban";
        const string projectionSchema = "projection_operational_sandbox_kanban";
        const string projectionTable = "kanban_read_model";
        const string aggregateType = "Kanban";

        // Use AddSingleton<IHostedService> instead of AddHostedService to avoid
        // TryAddEnumerable deduplication with Todo's same-type registration.
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
                sp.GetRequiredService<Whycespace.Shared.Contracts.Infrastructure.Messaging.KafkaConsumerOptions>(),
                sp.GetRequiredService<Whycespace.Shared.Contracts.Infrastructure.Health.IWorkerLivenessRegistry>(),
                sp.GetService<Microsoft.Extensions.Logging.ILogger<GenericKafkaProjectionConsumerWorker>>(),
                pollTimeout: null,
                deadLetterStore: sp.GetService<Whycespace.Shared.Contracts.Infrastructure.Messaging.IDeadLetterStore>(),
                // R2.A.D.3b: consumer DLQ publish flows through shared "kafka-producer" breaker.
                kafkaBreaker: sp.GetService<Whycespace.Shared.Contracts.Runtime.ICircuitBreakerRegistry>()?.TryGet("kafka-producer"),
                // R2.A.3d: retry tier escalation wiring.
                topicNameResolver: sp.GetService<Whycespace.Runtime.EventFabric.TopicNameResolver>(),
                retryOptions: sp.GetService<RetryTierOptions>(),
                randomProvider: sp.GetService<Whycespace.Shared.Kernel.Domain.IRandomProvider>()));

        return services;
    }

    public static void RegisterProjections(IServiceProvider provider, ProjectionRegistry projection)
    {
        var boardHandler = provider.GetRequiredService<KanbanBoardProjectionHandler>();
        projection.Register("KanbanBoardCreatedEvent", boardHandler);

        var listHandler = provider.GetRequiredService<KanbanListProjectionHandler>();
        projection.Register("KanbanListCreatedEvent", listHandler);

        var cardHandler = provider.GetRequiredService<KanbanCardProjectionHandler>();
        projection.Register("KanbanCardCreatedEvent", cardHandler);
        projection.Register("KanbanCardMovedEvent", cardHandler);
        projection.Register("KanbanCardReorderedEvent", cardHandler);
        projection.Register("KanbanCardCompletedEvent", cardHandler);
        projection.Register("KanbanCardDetailRevisedEvent", cardHandler);
    }
}
