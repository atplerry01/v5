using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whyce.Engines.T1M.Steps.Todo;
using Whyce.Engines.T2E.Operational.Todo;
using Whyce.Platform.Host.Adapters;
using Whyce.Projections.OperationalSystem.Sandbox.Todo;
using Whyce.Runtime.EventFabric;
using Whyce.Runtime.EventFabric.DomainSchemas;
using Whyce.Runtime.Projection;
using Whyce.Shared.Kernel.Domain;
using PostgresProjectionWriter = Whyce.Platform.Host.Adapters.PostgresProjectionWriter;
using Whyce.Shared.Contracts.Application.Todo;
using Whyce.Shared.Contracts.Engine;
using Whyce.Shared.Contracts.Runtime;
using Whyce.Systems.Downstream.OperationalSystem.Sandbox.Todo;

namespace Whyce.Platform.Host.Composition.Operational.Sandbox.Todo;

/// <summary>
/// Full domain wiring for operational/sandbox/todo. Owns every Todo-specific
/// DI registration, schema entry, projection mapping, engine binding, and
/// workflow definition that would otherwise live in Program.cs.
///
/// Schema identity binding is delegated to the runtime-side
/// DomainSchemaCatalog seam (Phase 1.5 §5.1.2 BPV-D01); host stays free of
/// typed domain-event references.
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
        // phase1.5-S1 (CFG-R3): no fallback — env var is required.
        // phase1.6-CFG-K1: Section:Key form (see InfrastructureComposition.cs)
        var projectionsCs = configuration.GetValue<string>("Postgres:ProjectionsConnectionString")
            ?? throw new InvalidOperationException(
                "Postgres:ProjectionsConnectionString is required. No fallback.");
        services.AddSingleton(sp => new TodoProjectionHandler(projectionsCs));
        services.AddSingleton<TodoProjectionConsumer>();

        // Systems.Downstream — Todo intent handler
        services.AddTransient<ITodoIntentHandler, TodoIntentHandler>();

        // Kafka projection consumer — generic worker.
        // Per-domain config (topic, group, projection table) lives here in the bootstrap module;
        // the worker itself contains zero domain references.
        var kafkaBootstrapServers = configuration.GetValue<string>("Kafka:BootstrapServers")
            ?? throw new InvalidOperationException("Kafka:BootstrapServers is required. No fallback.");
        // phase1.5-S1 (CFG-R3): no fallback — env var is required.
        var postgresProjectionsCs = configuration.GetValue<string>("Postgres:ProjectionsConnectionString")
            ?? throw new InvalidOperationException(
                "Postgres:ProjectionsConnectionString is required. No fallback.");

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
                    aggregateType),
                sp.GetRequiredService<IClock>(),
                sp.GetService<Microsoft.Extensions.Logging.ILogger<GenericKafkaProjectionConsumerWorker>>()));
    }

    public void RegisterSchema(EventSchemaRegistry schema)
    {
        // Phase 1.5 §5.1.2 BPV-D01: schema identity binding lives in the
        // runtime-side TodoSchemaModule. Host stays free of typed domain refs.
        DomainSchemaCatalog.RegisterOperationalSandboxTodo(schema);
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
