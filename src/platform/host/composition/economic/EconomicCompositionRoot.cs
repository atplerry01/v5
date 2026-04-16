using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Composition.Economic.Capital;
using Whycespace.Platform.Host.Composition.Economic.Compliance;
using Whycespace.Platform.Host.Composition.Economic.Risk;
using Whycespace.Platform.Host.Composition.Economic.Risk.Exposure.Application;
using Whycespace.Platform.Host.Composition.Economic.Capital.Account.Application;
using Whycespace.Platform.Host.Composition.Economic.Capital.Allocation.Application;
using Whycespace.Platform.Host.Composition.Economic.Capital.Asset.Application;
using Whycespace.Platform.Host.Composition.Economic.Capital.Binding.Application;
using Whycespace.Platform.Host.Composition.Economic.Capital.Pool.Application;
using Whycespace.Platform.Host.Composition.Economic.Capital.Reserve.Application;
using Whycespace.Platform.Host.Composition.Economic.Capital.Vault.Application;
using Whycespace.Platform.Host.Composition.Economic.Compliance.Audit.Application;
using Whycespace.Platform.Host.Composition.Economic.Enforcement;
using Whycespace.Platform.Host.Composition.Economic.Exchange;
using Whycespace.Platform.Host.Composition.Economic.Exchange.Application;
using Whycespace.Platform.Host.Composition.Economic.Enforcement.Escalation.Application;
using Whycespace.Platform.Host.Composition.Economic.Enforcement.Lock.Application;
using Whycespace.Platform.Host.Composition.Economic.Enforcement.Restriction.Application;
using Whycespace.Platform.Host.Composition.Economic.Enforcement.Rule.Application;
using Whycespace.Platform.Host.Composition.Economic.Enforcement.Sanction.Application;
using Whycespace.Platform.Host.Composition.Economic.Enforcement.Violation.Application;
using Whycespace.Platform.Host.Composition.Economic.Ledger;
using Whycespace.Platform.Host.Composition.Economic.Ledger.Entry.Application;
using Whycespace.Platform.Host.Composition.Economic.Ledger.Journal.Application;
using Whycespace.Platform.Host.Composition.Economic.Ledger.Ledger.Application;
using Whycespace.Platform.Host.Composition.Economic.Ledger.Obligation.Application;
using Whycespace.Platform.Host.Composition.Economic.Ledger.Treasury.Application;
using Whycespace.Platform.Host.Composition.Economic.Projection;
using Whycespace.Platform.Host.Composition.Economic.Reconciliation;
using Whycespace.Platform.Host.Composition.Economic.Reconciliation.Process.Application;
using Whycespace.Platform.Host.Composition.Economic.Reconciliation.Discrepancy.Application;
using Whycespace.Platform.Host.Composition.Economic.Reconciliation.Workflow;
using Whycespace.Platform.Host.Composition.Economic.Revenue.Distribution.Workflow;
using Whycespace.Platform.Host.Composition.Economic.Routing;
using Whycespace.Platform.Host.Composition.Economic.Routing.Execution.Application;
using Whycespace.Platform.Host.Composition.Economic.Routing.Path.Application;
using Whycespace.Platform.Host.Composition.Economic.Subject;
using Whycespace.Platform.Host.Composition.Economic.Subject.Subject.Application;
using Whycespace.Platform.Host.Composition.Economic.Transaction;
using Whycespace.Platform.Host.Composition.Economic.Revenue.Payout.Workflow;
using Whycespace.Platform.Host.Composition.Economic.Revenue.Revenue.Application;
using Whycespace.Platform.Host.Composition.Economic.Revenue.Revenue.Workflow;
using Whycespace.Platform.Host.Composition.Economic.Vault.Account.Application;
using Whycespace.Platform.Host.Composition.Economic.Vault.Account.Policy;
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
        services.AddRevenueRevenueApplication();
        services.AddRevenueProcessingWorkflow();
        services.AddDistributionWorkflow();
        services.AddPayoutExecutionWorkflow();
        services.AddLedgerEntryApplication();
        services.AddLedgerJournalApplication();
        services.AddLedgerLedgerApplication();
        services.AddLedgerObligationApplication();
        services.AddLedgerTreasuryApplication();

        // Phase 2 capital wiring (Option C) — application modules for all 7 capital contexts
        services.AddCapitalAccountApplication();
        services.AddCapitalAllocationApplication();
        services.AddCapitalAssetApplication();
        services.AddCapitalBindingApplication();
        services.AddCapitalPoolApplication();
        services.AddCapitalReserveApplication();
        services.AddCapitalVaultApplication();

        // Phase 2 risk wiring — application module for the risk/exposure domain
        services.AddRiskExposureApplication();

        services.AddComplianceAuditApplication();

        services.AddEnforcementRuleApplication();
        services.AddEnforcementViolationApplication();
        services.AddEnforcementEscalationApplication();
        services.AddEnforcementSanctionApplication();
        services.AddEnforcementRestrictionApplication();
        services.AddEnforcementLockApplication();

        // Phase 2 routing wiring — application modules for routing/path + routing/execution
        services.AddRoutingPathApplication();
        services.AddRoutingExecutionApplication();

        // Subject context application module (economic subject bridge)
        services.AddSubjectApplication();

        // Reconciliation context application modules (process + discrepancy)
        services.AddReconciliationProcessApplication();
        services.AddReconciliationDiscrepancyApplication();

        // Reconciliation lifecycle workflow (T1M orchestration).
        services.AddReconciliationWorkflow();

        // Transaction context application module (charge, expense, instruction,
        // limit, settlement, transaction, wallet).
        services.AddTransactionApplication();

        // Exchange context application module (fx + rate).
        services.AddExchangeApplication();

        // E5.1 — capital command → WHYCEPOLICY action bindings (29 entries).
        // Aggregated by ICommandPolicyIdRegistry so SystemIntentDispatcher
        // stamps CommandContext.PolicyId per command, enabling per-action
        // policy evaluation in PolicyMiddleware.
        services.AddLedgerPolicyBindings();
        services.AddCapitalPolicyBindings();
        services.AddRiskPolicyBindings();
        services.AddCompliancePolicyBindings();
        services.AddEnforcementPolicyBindings();
        services.AddRoutingPolicyBindings();
        services.AddSubjectPolicyBindings();
        services.AddExchangePolicyBindings();
        services.AddReconciliationPolicyBindings();
        services.AddVaultAccountPolicyBindings();

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

        // Phase 3 (enforcement) — Violation → Capital command bridge.
        // Consumes whyce.economic.enforcement.violation.events and, when the
        // RecommendedAction == Block, dispatches FreezeCapitalAccountCommand
        // against the subject (SourceReference) through the full runtime
        // pipeline. Softer actions (Warn / Restrict / Escalate) are handled
        // on the dispatch hot path by ExecutionGuardMiddleware via
        // IViolationStateQuery, so no command is dispatched for them.
        services.AddSingleton<EnforcementToCapitalIntegrationHandler>();

        services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(sp =>
            new EnforcementToCapitalWorker(
                kafkaBootstrapServers,
                sp.GetRequiredService<EventDeserializer>(),
                sp.GetRequiredService<EnforcementToCapitalIntegrationHandler>(),
                sp.GetRequiredService<IClock>(),
                sp.GetRequiredService<Whycespace.Shared.Contracts.Infrastructure.Messaging.KafkaConsumerOptions>(),
                sp.GetService<Microsoft.Extensions.Logging.ILogger<EnforcementToCapitalWorker>>()));

        // Phase 2 (enforcement) — Violation → Policy feedback bridge.
        // Consumes whyce.economic.enforcement.violation.events and publishes
        // PolicyFeedbackEventSchema to whyce.constitutional.policy.feedback.events
        // so WhycePolicy evaluations can factor in active enforcement state.
        var policyVersion = configuration.GetValue<string>("WhycePolicy:Version") ?? "v1";

        services.AddSingleton(sp => new EnforcementToPolicyFeedbackHandler(
            sp.GetRequiredService<Confluent.Kafka.IProducer<string, string>>(),
            sp.GetRequiredService<IClock>(),
            sp.GetRequiredService<IIdGenerator>(),
            policyVersion));

        services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(sp =>
            new EnforcementToPolicyFeedbackWorker(
                kafkaBootstrapServers,
                sp.GetRequiredService<EventDeserializer>(),
                sp.GetRequiredService<EnforcementToPolicyFeedbackHandler>(),
                sp.GetRequiredService<IClock>(),
                sp.GetRequiredService<Whycespace.Shared.Contracts.Infrastructure.Messaging.KafkaConsumerOptions>(),
                sp.GetService<Microsoft.Extensions.Logging.ILogger<EnforcementToPolicyFeedbackWorker>>()));

        // Phase 3 (enforcement) — OPA-backed detection worker.
        // Subscribes to a configurable set of source topics, evaluates each
        // envelope via the rego package data.whyce.enforcement.detect, and
        // dispatches DetectViolationCommand for each returned signal.
        var opaEndpoint = configuration.GetValue<string>("Opa:Endpoint")
            ?? throw new InvalidOperationException("Opa:Endpoint is required for enforcement detection.");
        var opaTimeoutMs = configuration.GetValue<int?>("Opa:RequestTimeoutMs") ?? 2000;

        services.AddHttpClient<OpaEnforcementEventEvaluator>();
        services.AddSingleton<Whycespace.Shared.Contracts.Enforcement.IEnforcementEventEvaluator>(sp =>
            new OpaEnforcementEventEvaluator(
                sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(OpaEnforcementEventEvaluator)),
                opaEndpoint,
                opaTimeoutMs,
                sp.GetService<Microsoft.Extensions.Logging.ILogger<OpaEnforcementEventEvaluator>>()));

        services.AddSingleton<EnforcementDetectionHandler>();

        // Phase 4 — active-violation read-side query for ExecutionGuardMiddleware.
        // Registered here (rather than runtime composition) because the projection
        // connection string is an economic-side infrastructure concern and the
        // violation projection is owned by this classification.
        var projectionsConnectionString = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException(
                "Projections:ConnectionString is required for Phase 4 enforcement integration.");
        services.AddSingleton<Whycespace.Shared.Contracts.Enforcement.IViolationStateQuery>(sp =>
            new PostgresViolationStateQuery(
                projectionsConnectionString,
                sp.GetService<Microsoft.Extensions.Logging.ILogger<PostgresViolationStateQuery>>()));

        services.AddSingleton<Whycespace.Shared.Contracts.Enforcement.IEscalationStateQuery>(sp =>
            new PostgresEscalationStateQuery(
                projectionsConnectionString,
                sp.GetService<Microsoft.Extensions.Logging.ILogger<PostgresEscalationStateQuery>>()));

        // Phase 1 (escalation) — Violation → Escalation bridge.
        // Consumes whyce.economic.enforcement.violation.events and dispatches
        // AccumulateViolationCommand per violation so the per-subject
        // ViolationEscalationAggregate can roll up counts, score, and level.
        services.AddSingleton<ViolationToEscalationHandler>();

        services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(sp =>
            new ViolationToEscalationWorker(
                kafkaBootstrapServers,
                sp.GetRequiredService<EventDeserializer>(),
                sp.GetRequiredService<ViolationToEscalationHandler>(),
                sp.GetRequiredService<IClock>(),
                sp.GetRequiredService<Whycespace.Shared.Contracts.Infrastructure.Messaging.KafkaConsumerOptions>(),
                sp.GetService<Microsoft.Extensions.Logging.ILogger<ViolationToEscalationWorker>>()));

        var detectionTopics = configuration.GetSection("Enforcement:Detection:Topics").Get<string[]>()
            ?? new[]
            {
                "whyce.economic.ledger.journal.events",
                "whyce.economic.ledger.ledger.events"
            };

        services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(sp =>
            new EnforcementDetectionWorker(
                kafkaBootstrapServers,
                detectionTopics,
                sp.GetRequiredService<EventDeserializer>(),
                sp.GetRequiredService<EnforcementDetectionHandler>(),
                sp.GetRequiredService<IClock>(),
                sp.GetRequiredService<Whycespace.Shared.Contracts.Infrastructure.Messaging.KafkaConsumerOptions>(),
                sp.GetService<Microsoft.Extensions.Logging.ILogger<EnforcementDetectionWorker>>()));

        // Phase 2.5 (reconciliation lifecycle workflow) — Kafka-driven T1M
        // orchestrator. Consumes whyce.economic.reconciliation.{process,discrepancy}.events
        // and dispatches the next T2E command via ISystemIntentDispatcher.
        services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(sp =>
            new ReconciliationLifecycleWorker(
                kafkaBootstrapServers,
                sp.GetRequiredService<EventDeserializer>(),
                sp.GetRequiredService<ReconciliationLifecycleHandler>(),
                sp.GetRequiredService<IClock>(),
                sp.GetRequiredService<Whycespace.Shared.Contracts.Infrastructure.Messaging.KafkaConsumerOptions>(),
                sp.GetRequiredService<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
                sp.GetService<Microsoft.Extensions.Logging.ILogger<ReconciliationLifecycleWorker>>()));
    }

    public void RegisterSchema(EventSchemaRegistry schema)
    {
        DomainSchemaCatalog.RegisterEconomic(schema);
    }

    public void RegisterProjections(IServiceProvider provider, ProjectionRegistry projection)
    {
        EconomicProjectionModule.RegisterProjections(provider, projection);
        ReconciliationWorkflowModule.RegisterProjections(provider, projection);
    }

    public void RegisterEngines(IEngineRegistry engine)
    {
        VaultAccountApplicationModule.RegisterEngines(engine);
        RevenueRevenueApplicationModule.RegisterEngines(engine);
        DistributionWorkflowModule.RegisterEngines(engine);
        LedgerEntryApplicationModule.RegisterEngines(engine);
        LedgerJournalApplicationModule.RegisterEngines(engine);
        LedgerLedgerApplicationModule.RegisterEngines(engine);
        LedgerObligationApplicationModule.RegisterEngines(engine);
        LedgerTreasuryApplicationModule.RegisterEngines(engine);

        // Phase 2 capital wiring (Option C) — engine registrations
        CapitalAccountApplicationModule.RegisterEngines(engine);
        CapitalAllocationApplicationModule.RegisterEngines(engine);
        CapitalAssetApplicationModule.RegisterEngines(engine);
        CapitalBindingApplicationModule.RegisterEngines(engine);
        CapitalPoolApplicationModule.RegisterEngines(engine);
        CapitalReserveApplicationModule.RegisterEngines(engine);
        CapitalVaultApplicationModule.RegisterEngines(engine);

        RiskExposureApplicationModule.RegisterEngines(engine);

        ComplianceAuditApplicationModule.RegisterEngines(engine);

        EnforcementRuleApplicationModule.RegisterEngines(engine);
        EnforcementViolationApplicationModule.RegisterEngines(engine);
        EnforcementEscalationApplicationModule.RegisterEngines(engine);
        EnforcementSanctionApplicationModule.RegisterEngines(engine);
        EnforcementRestrictionApplicationModule.RegisterEngines(engine);
        EnforcementLockApplicationModule.RegisterEngines(engine);

        RoutingPathApplicationModule.RegisterEngines(engine);
        RoutingExecutionApplicationModule.RegisterEngines(engine);

        SubjectApplicationModule.RegisterEngines(engine);

        TransactionApplicationModule.RegisterEngines(engine);

        ExchangeApplicationModule.RegisterEngines(engine);

        // Reconciliation context engine registrations (process + discrepancy)
        ReconciliationProcessApplicationModule.RegisterEngines(engine);
        ReconciliationDiscrepancyApplicationModule.RegisterEngines(engine);
    }

    public void RegisterWorkflows(IWorkflowRegistry workflow)
    {
        RevenueProcessingWorkflowModule.RegisterWorkflows(workflow);
        DistributionWorkflowModule.RegisterWorkflows(workflow);
        PayoutExecutionWorkflowModule.RegisterWorkflows(workflow);
    }
}
