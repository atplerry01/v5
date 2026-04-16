using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Adapters;
using Whycespace.Projections.Economic.Reconciliation.Workflow;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Projection;
using Whycespace.Shared.Contracts.Economic.Reconciliation.Workflow;

namespace Whycespace.Platform.Host.Composition.Economic.Reconciliation.Workflow;

/// <summary>
/// Composition for the T1M reconciliation lifecycle workflow.
///
/// Binds:
///   - ReconciliationLifecycleHandler (T1M event→command router)
///   - IReconciliationWorkflowStore / IReconciliationWorkflowLookup (Postgres-backed)
///   - ReconciliationWorkflowProjectionHandler (read-model materialisation)
///   - ReconciliationLifecycleWorker is registered in EconomicCompositionRoot
///     so it can grab the shared Kafka bootstrap + KafkaConsumerOptions.
/// </summary>
public static class ReconciliationWorkflowModule
{
    public static IServiceCollection AddReconciliationWorkflow(this IServiceCollection services)
    {
        services.AddSingleton<PostgresReconciliationWorkflowStore>();
        services.AddSingleton<IReconciliationWorkflowLookup>(sp =>
            sp.GetRequiredService<PostgresReconciliationWorkflowStore>());
        services.AddSingleton<IReconciliationWorkflowStore>(sp =>
            sp.GetRequiredService<PostgresReconciliationWorkflowStore>());

        services.AddSingleton<ReconciliationLifecycleHandler>();
        services.AddSingleton<ReconciliationWorkflowProjectionHandler>();

        return services;
    }

    public static void RegisterProjections(IServiceProvider provider, ProjectionRegistry projection)
    {
        var handler = provider.GetRequiredService<ReconciliationWorkflowProjectionHandler>();
        projection.Register("ReconciliationTriggeredEvent",  handler);
        projection.Register("ReconciliationMatchedEvent",    handler);
        projection.Register("ReconciliationMismatchedEvent", handler);
        projection.Register("ReconciliationResolvedEvent",   handler);
        projection.Register("DiscrepancyDetectedEvent",      handler);
        projection.Register("DiscrepancyInvestigatedEvent",  handler);
        projection.Register("DiscrepancyResolvedEvent",      handler);
    }
}
