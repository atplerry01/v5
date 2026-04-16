using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Adapters;
using Whycespace.Projections.Content.Learning.Course.Item;
using Whycespace.Projections.Shared;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Projection;
using Whycespace.Shared.Contracts.Content.Learning.Course;
using Whycespace.Shared.Kernel.Domain;
using PostgresProjectionWriter = Whycespace.Platform.Host.Adapters.PostgresProjectionWriter;

namespace Whycespace.Platform.Host.Composition.Content.Learning.Course.Projection;

/// <summary>
/// Phase 1 projection module for content-system/learning/course. Wires the
/// read-model store, projection handler, and Kafka consumer worker for the
/// canonical topic whyce.content.learning.course.events.
/// </summary>
public static class CourseProjectionModule
{
    public const string Topic = "whyce.content.learning.course.events";
    public const string ConsumerGroup = "whyce.projection.content.learning.course";
    public const string ProjectionSchema = "projection_content_learning_course";
    public const string ProjectionTable = "course_read_model";
    public const string AggregateType = "Course";

    public static IServiceCollection AddCourseProjection(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<CourseReadModel>(ProjectionSchema, ProjectionTable, AggregateType));
        services.AddSingleton(sp => new CourseProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<CourseReadModel>>()));

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
        var handler = provider.GetRequiredService<CourseProjectionHandler>();
        projection.Register("CourseDraftedEvent", handler);
        projection.Register("CourseModuleAttachedEvent", handler);
        projection.Register("CourseModuleDetachedEvent", handler);
        projection.Register("CoursePublishedEvent", handler);
        projection.Register("CourseArchivedEvent", handler);
    }
}
