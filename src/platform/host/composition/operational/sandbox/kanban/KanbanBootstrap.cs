using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whyce.Engines.T2E.Operational.Sandbox.Kanban;
using Whyce.Platform.Host.Adapters;
using Whyce.Runtime.EventFabric;
using Whyce.Runtime.EventFabric.DomainSchemas;
using Whyce.Runtime.Projection;
using Whyce.Shared.Contracts.Operational.Sandbox.Kanban;
using Whyce.Shared.Contracts.Engine;
using Whyce.Shared.Contracts.Runtime;
using Whyce.Shared.Kernel.Domain;
using Whyce.Projections.Operational.Sandbox.Kanban;
using Whyce.Systems.Downstream.Operational.Sandbox.Kanban;
using PostgresProjectionWriter = Whyce.Platform.Host.Adapters.PostgresProjectionWriter;

namespace Whyce.Platform.Host.Composition.Operational.Sandbox.Kanban;

/// <summary>
/// Full domain wiring for operational/sandbox/kanban. Owns every Kanban-specific
/// DI registration, schema entry, engine binding, and projection mapping.
/// </summary>
public sealed class KanbanBootstrap : IDomainBootstrapModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // T2E handlers (one per command)
        services.AddTransient<CreateKanbanBoardHandler>();
        services.AddTransient<CreateKanbanListHandler>();
        services.AddTransient<CreateKanbanCardHandler>();
        services.AddTransient<MoveKanbanCardHandler>();
        services.AddTransient<ReorderKanbanCardHandler>();
        services.AddTransient<CompleteKanbanCardHandler>();
        services.AddTransient<UpdateKanbanCardHandler>();

        // Systems.Downstream — Kanban intent handler
        services.AddTransient<IKanbanIntentHandler, KanbanIntentHandler>();

        // Projection layer
        services.AddSingleton(sp => new KanbanProjectionHandler(
            sp.GetRequiredService<ProjectionsDataSource>().Inner));

        // Kafka projection consumer
        var kafkaBootstrapServers = configuration.GetValue<string>("Kafka:BootstrapServers")
            ?? throw new InvalidOperationException("Kafka:BootstrapServers is required. No fallback.");

        const string topic = "whyce.operational.sandbox.kanban.events";
        const string consumerGroup = "whyce.projection.operational.sandbox.kanban";
        const string projectionSchema = "projection_operational_sandbox_kanban";
        const string projectionTable = "kanban_read_model";
        const string aggregateType = "Kanban";

        // Use AddSingleton<IHostedService> instead of AddHostedService to avoid
        // TryAddEnumerable deduplication with TodoBootstrap's same-type registration.
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
                sp.GetRequiredService<Whyce.Shared.Contracts.Infrastructure.Messaging.KafkaConsumerOptions>(),
                sp.GetRequiredService<Whyce.Shared.Contracts.Infrastructure.Health.IWorkerLivenessRegistry>(),
                sp.GetService<Microsoft.Extensions.Logging.ILogger<GenericKafkaProjectionConsumerWorker>>()));
    }

    public void RegisterSchema(EventSchemaRegistry schema)
    {
        DomainSchemaCatalog.RegisterOperationalSandboxKanban(schema);
    }

    public void RegisterProjections(IServiceProvider provider, ProjectionRegistry projection)
    {
        var handler = provider.GetRequiredService<KanbanProjectionHandler>();

        projection.Register("KanbanBoardCreatedEvent", handler);
        projection.Register("KanbanListCreatedEvent", handler);
        projection.Register("KanbanCardCreatedEvent", handler);
        projection.Register("KanbanCardMovedEvent", handler);
        projection.Register("KanbanCardReorderedEvent", handler);
        projection.Register("KanbanCardCompletedEvent", handler);
        projection.Register("KanbanCardUpdatedEvent", handler);
    }

    public void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateKanbanBoardCommand, CreateKanbanBoardHandler>();
        engine.Register<CreateKanbanListCommand, CreateKanbanListHandler>();
        engine.Register<CreateKanbanCardCommand, CreateKanbanCardHandler>();
        engine.Register<MoveKanbanCardCommand, MoveKanbanCardHandler>();
        engine.Register<ReorderKanbanCardCommand, ReorderKanbanCardHandler>();
        engine.Register<CompleteKanbanCardCommand, CompleteKanbanCardHandler>();
        engine.Register<UpdateKanbanCardCommand, UpdateKanbanCardHandler>();
    }

    public void RegisterWorkflows(IWorkflowRegistry workflow)
    {
        // No T1M workflows for Kanban yet
    }
}
