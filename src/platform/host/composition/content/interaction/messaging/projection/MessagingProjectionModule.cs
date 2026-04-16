using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Adapters;
using Whycespace.Projections.Content.Interaction.Messaging.Item;
using Whycespace.Projections.Shared;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Projection;
using Whycespace.Shared.Contracts.Content.Interaction.Messaging;
using Whycespace.Shared.Kernel.Domain;
using PostgresProjectionWriter = Whycespace.Platform.Host.Adapters.PostgresProjectionWriter;

namespace Whycespace.Platform.Host.Composition.Content.Interaction.Messaging.Projection;

/// <summary>
/// Phase 1 projection module for content-system/interaction/messaging.
/// Wires the read-model store, projection handler, and Kafka consumer worker
/// for the canonical topic whyce.content.interaction.messaging.events.
/// </summary>
public static class MessagingProjectionModule
{
    public const string Topic = "whyce.content.interaction.messaging.events";
    public const string ConsumerGroup = "whyce.projection.content.interaction.messaging";
    public const string ProjectionSchema = "projection_content_interaction_messaging";
    public const string ProjectionTable = "message_read_model";
    public const string AggregateType = "Message";

    public static IServiceCollection AddMessagingProjection(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<MessageReadModel>(ProjectionSchema, ProjectionTable, AggregateType));
        services.AddSingleton(sp => new MessageProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<MessageReadModel>>()));

        var kafkaBootstrapServers = configuration.GetValue<string>("Kafka:BootstrapServers")
            ?? throw new InvalidOperationException("Kafka:BootstrapServers is required. No fallback.");

        services.AddHostedService(sp =>
            new GenericKafkaProjectionConsumerWorker(
                kafkaBootstrapServers,
                Topic,
                ConsumerGroup,
                sp.GetRequiredService<EventDeserializer>(),
                sp.GetRequiredService<ProjectionRegistry>(),
                new PostgresProjectionWriter(
                    sp.GetRequiredService<ProjectionsDataSource>(),
                    ProjectionSchema,
                    ProjectionTable,
                    AggregateType),
                sp.GetRequiredService<IClock>(),
                sp.GetRequiredService<Whycespace.Shared.Contracts.Infrastructure.Messaging.KafkaConsumerOptions>(),
                sp.GetRequiredService<Whycespace.Shared.Contracts.Infrastructure.Health.IWorkerLivenessRegistry>(),
                sp.GetService<Microsoft.Extensions.Logging.ILogger<GenericKafkaProjectionConsumerWorker>>()));

        return services;
    }

    public static void RegisterProjections(IServiceProvider provider, ProjectionRegistry projection)
    {
        var handler = provider.GetRequiredService<MessageProjectionHandler>();
        projection.Register("MessageSentEvent", handler);
        projection.Register("MessageDeliveredEvent", handler);
        projection.Register("MessageReadEvent", handler);
        projection.Register("MessageEditedEvent", handler);
        projection.Register("MessageRetractedEvent", handler);
    }
}
