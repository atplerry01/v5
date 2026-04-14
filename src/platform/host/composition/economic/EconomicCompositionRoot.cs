using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Composition.Economic.Capital.Account.Application;
using Whycespace.Platform.Host.Composition.Economic.Capital.Allocation.Application;
using Whycespace.Platform.Host.Composition.Economic.Capital.Asset.Application;
using Whycespace.Platform.Host.Composition.Economic.Capital.Binding.Application;
using Whycespace.Platform.Host.Composition.Economic.Capital.Pool.Application;
using Whycespace.Platform.Host.Composition.Economic.Capital.Reserve.Application;
using Whycespace.Platform.Host.Composition.Economic.Capital.Vault.Application;
using Whycespace.Platform.Host.Composition.Economic.Ledger.Journal.Application;
using Whycespace.Platform.Host.Composition.Economic.Projection;
using Whycespace.Platform.Host.Composition.Economic.Revenue.Distribution.Workflow;
using Whycespace.Platform.Host.Composition.Economic.Revenue.Payout.Workflow;
using Whycespace.Platform.Host.Composition.Economic.Revenue.Revenue.Workflow;
using Whycespace.Platform.Host.Composition.Economic.Vault.Account.Application;
using Whycespace.Platform.Host.Adapters;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.EventFabric.DomainSchemas;
using Whycespace.Runtime.Projection;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Composition.Economic;

/// <summary>
/// Composition root for the economic classification (revenue + vault/account).
/// Wires Phase 2D workflow/application modules, Phase 2E projection module,
/// and Phase 2E.1 workflow-trigger worker into the host lifecycle via the
/// IDomainBootstrapModule contract.
/// </summary>
public sealed class EconomicCompositionRoot : IDomainBootstrapModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // Phase 2D — application + workflow DI
        services.AddVaultAccountApplication();
        services.AddRevenueProcessingWorkflow();
        services.AddDistributionWorkflow();
        services.AddPayoutExecutionWorkflow();
        services.AddLedgerJournalApplication();

        // Phase 2 capital wiring (Option C) — application modules for all 7 capital contexts
        services.AddCapitalAccountApplication();
        services.AddCapitalAllocationApplication();
        services.AddCapitalAssetApplication();
        services.AddCapitalBindingApplication();
        services.AddCapitalPoolApplication();
        services.AddCapitalReserveApplication();
        services.AddCapitalVaultApplication();

        // Phase 2E — projection stores + handlers + 4 per-topic consumer workers
        services.AddEconomicProjection(configuration);

        // Phase 2E.1 — event→workflow trigger worker
        services.AddSingleton<WorkflowTriggerHandler>();

        var kafkaBootstrapServers = configuration.GetValue<string>("Kafka:BootstrapServers")
            ?? throw new InvalidOperationException("Kafka:BootstrapServers is required. No fallback.");

        services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(sp =>
            new WorkflowTriggerWorker(
                kafkaBootstrapServers,
                sp.GetRequiredService<EventDeserializer>(),
                sp.GetRequiredService<WorkflowTriggerHandler>(),
                sp.GetRequiredService<IClock>(),
                sp.GetRequiredService<Whycespace.Shared.Contracts.Infrastructure.Messaging.KafkaConsumerOptions>(),
                sp.GetService<Microsoft.Extensions.Logging.ILogger<WorkflowTriggerWorker>>()));

        // Phase 2 — Ledger → Capital event-driven integration.
        // Subscribes to whyce.economic.ledger.{journal,ledger}.events and dispatches
        // capital-account mutations via ISystemIntentDispatcher. No direct
        // ledger↔capital coupling: the worker consumes shared-contract event
        // schemas and emits shared-contract capital-account commands.
        services.AddSingleton<LedgerToCapitalIntegrationHandler>();

        services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(sp =>
            new LedgerToCapitalIntegrationWorker(
                kafkaBootstrapServers,
                sp.GetRequiredService<EventDeserializer>(),
                sp.GetRequiredService<LedgerToCapitalIntegrationHandler>(),
                sp.GetRequiredService<IClock>(),
                sp.GetRequiredService<Whycespace.Shared.Contracts.Infrastructure.Messaging.KafkaConsumerOptions>(),
                sp.GetService<Microsoft.Extensions.Logging.ILogger<LedgerToCapitalIntegrationWorker>>()));
    }

    public void RegisterSchema(EventSchemaRegistry schema)
    {
        DomainSchemaCatalog.RegisterEconomic(schema);
    }

    public void RegisterProjections(IServiceProvider provider, ProjectionRegistry projection)
    {
        EconomicProjectionModule.RegisterProjections(provider, projection);
    }

    public void RegisterEngines(IEngineRegistry engine)
    {
        VaultAccountApplicationModule.RegisterEngines(engine);
        DistributionWorkflowModule.RegisterEngines(engine);
        LedgerJournalApplicationModule.RegisterEngines(engine);

        // Phase 2 capital wiring (Option C) — engine registrations
        CapitalAccountApplicationModule.RegisterEngines(engine);
        CapitalAllocationApplicationModule.RegisterEngines(engine);
        CapitalAssetApplicationModule.RegisterEngines(engine);
        CapitalBindingApplicationModule.RegisterEngines(engine);
        CapitalPoolApplicationModule.RegisterEngines(engine);
        CapitalReserveApplicationModule.RegisterEngines(engine);
        CapitalVaultApplicationModule.RegisterEngines(engine);
    }

    public void RegisterWorkflows(IWorkflowRegistry workflow)
    {
        RevenueProcessingWorkflowModule.RegisterWorkflows(workflow);
        DistributionWorkflowModule.RegisterWorkflows(workflow);
        PayoutExecutionWorkflowModule.RegisterWorkflows(workflow);
    }
}
