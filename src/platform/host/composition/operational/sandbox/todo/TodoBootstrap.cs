using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whyce.Engines.T1M.Steps.Todo;
using Whyce.Engines.T2E.Operational.Todo;
using Whyce.Platform.Host.Adapters;
using Whyce.Projections.OperationalSystem.Sandbox.Todo;
using Whyce.Runtime.EventFabric;
using Whyce.Runtime.Projection;
using PostgresProjectionWriter = Whyce.Platform.Host.Adapters.PostgresProjectionWriter;
using Whyce.Shared.Contracts.Application.Todo;
using Whyce.Shared.Contracts.Engine;
using Whyce.Shared.Contracts.Events.Todo;
using Whyce.Shared.Contracts.Runtime;
using Whyce.Systems.Downstream.OperationalSystem.Sandbox.Todo;

namespace Whyce.Platform.Host.Composition.Operational.Sandbox.Todo;

/// <summary>
/// Phase B2a: full domain wiring for operational/sandbox/todo.
/// Owns every Todo-specific DI registration, schema entry, projection mapping,
/// engine binding, and workflow definition that previously lived in Program.cs.
///
/// Behavior is intentionally identical to the prior inline wiring — only the
/// composition site moves. Phase B2b will:
///   - Add EventSchemaEntry.ClrType + register CLR types here
///   - Replace KafkaProjectionConsumerWorker with a generic worker
///   - Remove EventTypeResolver and resolve via the schema registry instead
/// </summary>
public sealed class TodoBootstrap : IDomainBootstrapModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // T1M workflow steps
        services.AddTransient<ValidateIntentStep>();
        services.AddTransient<CreateTodoStep>();
        services.AddTransient<EmitCompletionStep>();

        // T2E engine
        services.AddTransient<TodoEngine>();

        // Projection layer (consumers receive events from Kafka ONLY)
        var projectionsCs = configuration.GetValue<string>("Postgres__ProjectionsConnectionString")
            ?? "Host=localhost;Port=5434;Database=whyce_projections;Username=whyce;Password=whyce";
        services.AddSingleton(sp => new TodoProjectionHandler(projectionsCs));
        services.AddSingleton<TodoProjectionConsumer>();

        // Systems.Downstream — Todo intent handler
        services.AddTransient<ITodoIntentHandler, TodoIntentHandler>();

        // Kafka projection consumer — generic worker (Phase B2b).
        // Per-domain config (topic, group, projection table) lives here in the bootstrap module;
        // the worker itself contains zero domain references.
        var kafkaBootstrapServers = configuration.GetValue<string>("Kafka__BootstrapServers")
            ?? throw new InvalidOperationException("Kafka__BootstrapServers is required. No fallback.");
        var postgresProjectionsCs = configuration.GetValue<string>("Postgres__ProjectionsConnectionString")
            ?? "Host=localhost;Port=5434;Database=whyce_projections;Username=whyce;Password=whyce";

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
                    postgresProjectionsCs,
                    projectionSchema,
                    projectionTable,
                    aggregateType)));
    }

    public void RegisterSchema(EventSchemaRegistry schema)
    {
        // Phase B2b: dual CLR types — stored = domain event (event store replay),
        // inbound = schema contract (Kafka consumer post payload mapping).
        schema.Register(
            "TodoCreatedEvent",
            EventVersion.Default,
            typeof(Whycespace.Domain.OperationalSystem.Sandbox.Todo.TodoCreatedEvent),
            typeof(TodoCreatedEventSchema));
        schema.Register(
            "TodoUpdatedEvent",
            EventVersion.Default,
            typeof(Whycespace.Domain.OperationalSystem.Sandbox.Todo.TodoUpdatedEvent),
            typeof(TodoUpdatedEventSchema));
        schema.Register(
            "TodoCompletedEvent",
            EventVersion.Default,
            typeof(Whycespace.Domain.OperationalSystem.Sandbox.Todo.TodoCompletedEvent),
            typeof(TodoCompletedEventSchema));

        // Payload mappers: domain events → shared schema contracts (projection layer isolation).
        // Used by EventFabric outbound publish path; deserialization for inbound now uses InboundEventType above.
        schema.RegisterPayloadMapper("TodoCreatedEvent", e =>
        {
            var evt = (Whycespace.Domain.OperationalSystem.Sandbox.Todo.TodoCreatedEvent)e;
            return new TodoCreatedEventSchema(evt.AggregateId.Value, evt.Title);
        });
        schema.RegisterPayloadMapper("TodoUpdatedEvent", e =>
        {
            var evt = (Whycespace.Domain.OperationalSystem.Sandbox.Todo.TodoUpdatedEvent)e;
            return new TodoUpdatedEventSchema(evt.AggregateId.Value, evt.Title);
        });
        schema.RegisterPayloadMapper("TodoCompletedEvent", e =>
        {
            var evt = (Whycespace.Domain.OperationalSystem.Sandbox.Todo.TodoCompletedEvent)e;
            return new TodoCompletedEventSchema(evt.AggregateId.Value);
        });
    }

    public void RegisterProjections(IServiceProvider provider, ProjectionRegistry projection)
    {
        var handler = provider.GetRequiredService<TodoProjectionHandler>();

        projection.Register("TodoCreatedEvent", handler);
        projection.Register("TodoUpdatedEvent", handler);
        projection.Register("TodoCompletedEvent", handler);
    }

    public void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateTodoCommand, TodoEngine>();
        engine.Register<UpdateTodoCommand, TodoEngine>();
        engine.Register<CompleteTodoCommand, TodoEngine>();
    }

    public void RegisterWorkflows(IWorkflowRegistry workflow)
    {
        workflow.Register("todo.lifecycle.create", new[]
        {
            typeof(ValidateIntentStep),
            typeof(CreateTodoStep),
            typeof(EmitCompletionStep)
        });
    }
}
