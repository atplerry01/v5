using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whyce.Engines.T1M.Domains.Operational.Sandbox.Kanban.Steps;
using Whyce.Engines.T1M.Domains.Operational.Sandbox.Kanban.Workflows;
using Whyce.Engines.T2E.Operational.Sandbox.Kanban;
using Whyce.Platform.Host.Adapters;
using Whyce.Runtime.EventFabric;
using Whyce.Runtime.EventFabric.DomainSchemas;
using Whyce.Runtime.Projection;
using Whyce.Shared.Contracts.Operational.Sandbox.Kanban;
using Whyce.Shared.Contracts.Engine;
using Whyce.Shared.Contracts.Runtime;
using Whyce.Shared.Kernel.Domain;
using Whyce.Projections.Operational.Sandbox.Kanban.Board;
using Whyce.Projections.Operational.Sandbox.Kanban.List;
using Whyce.Projections.Operational.Sandbox.Kanban.Card;
using Whyce.Projections.Shared;
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

        // T1M workflow steps (card approval lifecycle)
        services.AddTransient<ValidateCardStep>();
        services.AddTransient<MoveToReviewStep>();
        services.AddTransient<ApproveCardStep>();
        services.AddTransient<CompleteCardStep>();

        // Systems.Downstream — Kanban intent handler
        services.AddTransient<IKanbanIntentHandler, KanbanIntentHandler>();

        // Projection layer — factory-created store + domain-focused handlers
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
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
        workflow.Register(CardApprovalWorkflowNames.Approve, new[]
        {
            typeof(ValidateCardStep),
            typeof(MoveToReviewStep),
            typeof(ApproveCardStep),
            typeof(CompleteCardStep)
        });
    }
}
