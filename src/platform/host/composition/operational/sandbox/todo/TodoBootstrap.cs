using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whyce.Engines.T2E.Operational.Sandbox.Todo;
using Whyce.Platform.Host.Adapters;
using Whyce.Projections.Operational.Sandbox.Todo.Item;
using Whyce.Projections.Shared;
using Whyce.Runtime.EventFabric;
using Whyce.Runtime.EventFabric.DomainSchemas;
using Whyce.Runtime.Projection;
using Whyce.Shared.Kernel.Domain;
using PostgresProjectionWriter = Whyce.Platform.Host.Adapters.PostgresProjectionWriter;
using Whyce.Shared.Contracts.Operational.Sandbox.Todo;
using Whyce.Shared.Contracts.Engine;
using Whyce.Shared.Contracts.Runtime;
using Whyce.Systems.Downstream.Operational.Sandbox.Todo;

namespace Whyce.Platform.Host.Composition.Operational.Sandbox.Todo;

/// <summary>
/// Full domain wiring for operational/sandbox/todo. Owns every Todo-specific
/// DI registration, schema entry, projection mapping, engine binding, and
/// workflow definition that would otherwise live in Program.cs.
/// </summary>
public sealed class TodoBootstrap : IDomainBootstrapModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // T2E handlers (one per command)
        services.AddTransient<CreateTodoHandler>();
        services.AddTransient<UpdateTodoHandler>();
        services.AddTransient<CompleteTodoHandler>();

        // Projection layer — factory-created store + handler
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<TodoReadModel>("projection_operational_sandbox_todo", "todo_read_model", "Todo"));
        services.AddSingleton(sp => new TodoProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<TodoReadModel>>()));

        // Systems.Downstream — Todo intent handler
        services.AddTransient<ITodoIntentHandler, TodoIntentHandler>();

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
                sp.GetRequiredService<Whyce.Shared.Contracts.Infrastructure.Messaging.KafkaConsumerOptions>(),
                sp.GetRequiredService<Whyce.Shared.Contracts.Infrastructure.Health.IWorkerLivenessRegistry>(),
                sp.GetService<Microsoft.Extensions.Logging.ILogger<GenericKafkaProjectionConsumerWorker>>()));
    }

    public void RegisterSchema(EventSchemaRegistry schema)
    {
        DomainSchemaCatalog.RegisterOperationalSandboxTodo(schema);
    }

    public void RegisterProjections(IServiceProvider provider, ProjectionRegistry projection)
    {
        var handler = provider.GetRequiredService<TodoProjectionHandler>();

        projection.Register("TodoCreatedEvent", handler);
        projection.Register("TodoTitleRevisedEvent", handler);
        projection.Register("TodoCompletedEvent", handler);
    }

    public void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateTodoCommand, CreateTodoHandler>();
        engine.Register<UpdateTodoCommand, UpdateTodoHandler>();
        engine.Register<CompleteTodoCommand, CompleteTodoHandler>();
    }

    public void RegisterWorkflows(IWorkflowRegistry workflow)
    {
        // No T1M workflows for Todo — all commands dispatch directly to T2E.
    }
}
