using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Adapters;
using Whycespace.Projections.Content.Media.Asset.Item;
using Whycespace.Projections.Shared;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Projection;
using Whycespace.Shared.Contracts.Content.Media.Asset;
using Whycespace.Shared.Kernel.Domain;
using PostgresProjectionWriter = Whycespace.Platform.Host.Adapters.PostgresProjectionWriter;

namespace Whycespace.Platform.Host.Composition.Content.Media.Asset.Projection;

/// <summary>
/// Phase 1 projection module for content-system/media/asset.
/// Wires the read-model store, projection handler, and Kafka consumer worker
/// for the canonical topic whyce.content.media.asset.events.
/// </summary>
public static class MediaAssetProjectionModule
{
    public const string Topic = "whyce.content.media.asset.events";
    public const string ConsumerGroup = "whyce.projection.content.media.asset";
    public const string ProjectionSchema = "projection_content_media_asset";
    public const string ProjectionTable = "media_asset_read_model";
    public const string AggregateType = "MediaAsset";

    public static IServiceCollection AddMediaAssetProjection(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<MediaAssetReadModel>(ProjectionSchema, ProjectionTable, AggregateType));
        services.AddSingleton(sp => new MediaAssetProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<MediaAssetReadModel>>()));

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
        var handler = provider.GetRequiredService<MediaAssetProjectionHandler>();
        projection.Register("MediaAssetRegisteredEvent", handler);
        projection.Register("MediaAssetProcessingStartedEvent", handler);
        projection.Register("MediaAssetAvailableEvent", handler);
        projection.Register("MediaAssetPublishedEvent", handler);
        projection.Register("MediaAssetUnpublishedEvent", handler);
        projection.Register("MediaAssetArchivedEvent", handler);
        projection.Register("MediaAssetMetadataUpdatedEvent", handler);
    }
}
