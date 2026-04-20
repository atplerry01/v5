using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Adapters;
using Whycespace.Projections.Operational.Sandbox.Todo.Item;
using Whycespace.Projections.Shared;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Projection;
using Whycespace.Shared.Contracts.Operational.Sandbox.Todo;
using Whycespace.Shared.Kernel.Domain;
using PostgresProjectionWriter = Whycespace.Platform.Host.Adapters.PostgresProjectionWriter;

namespace Whycespace.Platform.Host.Composition.Operational.Sandbox.Todo.Projection;

/// <summary>
/// Todo projection module — projection handler DI registrations, store factory,
/// Kafka consumer, and projection registry bindings.
/// </summary>
public static class TodoProjectionModule
{
    public static IServiceCollection AddTodoProjection(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Projection layer — factory-created store + handler
        services.AddSingleton(sp =>
            sp.GetRequiredService<ProjectionStoreFactory>()
                .Create<TodoReadModel>("projection_operational_sandbox_todo", "todo_read_model", "Todo"));
        services.AddSingleton(sp => new TodoProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<TodoReadModel>>()));

        // Kafka projection consumer
        var kafkaBootstrapServers = configuration.GetValue<string>("Kafka:BootstrapServers")
            ?? throw new InvalidOperationException("Kafka:BootstrapServers is required. No fallback.");

        const string topic = "whyce.operational.sandbox.todo.events";
        const string consumerGroup = "whyce.projection.operational.sandbox.todo";
        const string projectionSchema = "projection_operational_sandbox_todo";
        const string projectionTable = "todo_read_model";
        const string aggregateType = "Todo";

        services.AddHostedService(sp =>
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
        var handler = provider.GetRequiredService<TodoProjectionHandler>();

        projection.Register("TodoCreatedEvent", handler);
        projection.Register("TodoTitleRevisedEvent", handler);
        projection.Register("TodoCompletedEvent", handler);
    }
}
