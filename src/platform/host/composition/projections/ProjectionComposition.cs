using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T0U.WhyceChain.Engine;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Projection;
using Whycespace.Shared.Contracts.Infrastructure.Chain;
using Whycespace.Shared.Contracts.Infrastructure.Messaging;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Composition.Projections;

/// <summary>
/// Projection registry, event-fabric services, schema registry, and the EventFabric
/// orchestrator itself. Domain projections, projection handlers, and the Kafka
/// projection consumer worker are registered via the per-domain bootstrap modules.
/// </summary>
public static class ProjectionComposition
{
    public static IServiceCollection AddProjectionComposition(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // --- Projection Registry (Event Fabric dispatches to projections) ---
        // Populated by domain bootstrap modules INSIDE the factory closure, BEFORE Lock() —
        // preserves the lock-after-build immutability guarantee.
        services.AddSingleton<ProjectionRegistry>(sp =>
        {
            var registry = new ProjectionRegistry();
            foreach (var module in sp.GetServices<IDomainBootstrapModule>())
                module.RegisterProjections(sp, registry);
            registry.Lock();
            return registry;
        });

        // --- Event Fabric Services (split responsibilities) ---
        services.AddSingleton<EventStoreService>(sp =>
            new EventStoreService(sp.GetRequiredService<IEventStore>()));
        services.AddSingleton<ChainAnchorService>(sp =>
            new ChainAnchorService(
                sp.GetRequiredService<WhyceChainEngine>(),
                sp.GetRequiredService<IChainAnchor>(),
                sp.GetRequiredService<Whycespace.Shared.Contracts.Infrastructure.Admission.ChainAnchorOptions>()));
        services.AddSingleton<OutboxService>(sp =>
            new OutboxService(sp.GetRequiredService<IOutbox>()));
        services.AddSingleton<EventSchemaRegistry>(sp =>
        {
            var registry = new EventSchemaRegistry();
            foreach (var module in sp.GetServices<IDomainBootstrapModule>())
                module.RegisterSchema(registry);
            registry.Lock();
            return registry;
        });

        // --- Event Fabric (orchestrator ONLY — delegates to services, NO direct projection dispatch) ---
        services.AddSingleton<TopicNameResolver>();
        services.AddSingleton<IEventFabric>(sp =>
            new EventFabric(
                sp.GetRequiredService<EventStoreService>(),
                sp.GetRequiredService<ChainAnchorService>(),
                sp.GetRequiredService<OutboxService>(),
                sp.GetRequiredService<EventSchemaRegistry>(),
                sp.GetRequiredService<TopicNameResolver>(),
                sp.GetRequiredService<IClock>()));

        return services;
    }
}
