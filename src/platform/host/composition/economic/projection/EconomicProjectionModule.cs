using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Adapters;
using Whycespace.Projections.Economic.Capital.Account;
using Whycespace.Projections.Economic.Capital.Allocation;
using Whycespace.Projections.Economic.Capital.Asset;
using Whycespace.Projections.Economic.Capital.Binding;
using Whycespace.Projections.Economic.Capital.Pool;
using Whycespace.Projections.Economic.Capital.Reserve;
using Whycespace.Projections.Economic.Capital.Vault;
using Whycespace.Projections.Economic.Compliance.Audit;
using Whycespace.Projections.Economic.Enforcement.Escalation;
using Whycespace.Projections.Economic.Enforcement.Lock;
using Whycespace.Projections.Economic.Enforcement.Restriction;
using Whycespace.Projections.Economic.Enforcement.Rule;
using Whycespace.Projections.Economic.Enforcement.Sanction;
using Whycespace.Projections.Economic.Enforcement.Violation;
using Whycespace.Projections.Economic.Exchange.Fx;
using Whycespace.Projections.Economic.Exchange.Rate;
using Whycespace.Projections.Economic.Ledger.Entry;
using Whycespace.Projections.Economic.Ledger.Ledger;
using Whycespace.Projections.Economic.Ledger.Obligation;
using Whycespace.Projections.Economic.Ledger.Treasury;
using Whycespace.Projections.Economic.Reconciliation.Process;
using Whycespace.Projections.Economic.Reconciliation.Discrepancy;
using Whycespace.Projections.Economic.Risk.Exposure;
using Whycespace.Projections.Economic.Routing.Execution;
using Whycespace.Projections.Economic.Routing.Path;
using Whycespace.Projections.Economic.Subject.Subject;
using Whycespace.Projections.Economic.Revenue.Distribution;
using Whycespace.Projections.Economic.Revenue.Payout;
using Whycespace.Projections.Economic.Revenue.Revenue;
using Whycespace.Projections.Economic.Transaction.Charge;
using Whycespace.Projections.Economic.Transaction.Expense;
using Whycespace.Projections.Economic.Transaction.Instruction;
using Whycespace.Projections.Economic.Transaction.Limit;
using Whycespace.Projections.Economic.Transaction.Settlement;
using Whycespace.Projections.Economic.Transaction.Transaction;
using Whycespace.Projections.Economic.Transaction.Wallet;
using Whycespace.Projections.Economic.Vault.Account;
using Whycespace.Projections.Shared;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Projection;
using Whycespace.Shared.Contracts.Economic.Capital.Account;
using Whycespace.Shared.Contracts.Economic.Capital.Allocation;
using Whycespace.Shared.Contracts.Economic.Capital.Asset;
using Whycespace.Shared.Contracts.Economic.Capital.Binding;
using Whycespace.Shared.Contracts.Economic.Capital.Pool;
using Whycespace.Shared.Contracts.Economic.Capital.Reserve;
using Whycespace.Shared.Contracts.Economic.Capital.Vault;
using Whycespace.Shared.Contracts.Economic.Compliance.Audit;
using Whycespace.Shared.Contracts.Economic.Enforcement.Escalation;
using Whycespace.Shared.Contracts.Economic.Enforcement.Lock;
using Whycespace.Shared.Contracts.Economic.Enforcement.Restriction;
using Whycespace.Shared.Contracts.Economic.Enforcement.Rule;
using Whycespace.Shared.Contracts.Economic.Enforcement.Sanction;
using Whycespace.Shared.Contracts.Economic.Enforcement.Violation;
using Whycespace.Shared.Contracts.Economic.Exchange.Fx;
using Whycespace.Shared.Contracts.Economic.Exchange.Rate;
using Whycespace.Shared.Contracts.Economic.Ledger.Entry;
using Whycespace.Shared.Contracts.Economic.Ledger.Ledger;
using Whycespace.Shared.Contracts.Economic.Ledger.Obligation;
using Whycespace.Shared.Contracts.Economic.Ledger.Treasury;
using Whycespace.Shared.Contracts.Economic.Reconciliation.Process;
using Whycespace.Shared.Contracts.Economic.Reconciliation.Discrepancy;
using Whycespace.Shared.Contracts.Economic.Revenue.Distribution;
using Whycespace.Shared.Contracts.Economic.Risk.Exposure;
using Whycespace.Shared.Contracts.Economic.Routing.Execution;
using Whycespace.Shared.Contracts.Economic.Routing.Path;
using Whycespace.Shared.Contracts.Economic.Subject.Subject;
using Whycespace.Shared.Contracts.Economic.Revenue.Payout;
using Whycespace.Shared.Contracts.Economic.Revenue.Revenue;
using Whycespace.Shared.Contracts.Economic.Transaction.Charge;
using Whycespace.Shared.Contracts.Economic.Transaction.Expense;
using Whycespace.Shared.Contracts.Economic.Transaction.Instruction;
using Whycespace.Shared.Contracts.Economic.Transaction.Limit;
using Whycespace.Shared.Contracts.Economic.Transaction.Settlement;
using Whycespace.Shared.Contracts.Economic.Transaction.Transaction;
using Whycespace.Shared.Contracts.Economic.Transaction.Wallet;
using Whycespace.Shared.Contracts.Economic.Vault.Account;
using Whycespace.Shared.Kernel.Domain;
using PostgresProjectionWriter = Whycespace.Platform.Host.Adapters.PostgresProjectionWriter;

namespace Whycespace.Platform.Host.Composition.Economic.Projection;

/// <summary>
/// Economic projection module — registers four PostgresProjectionStore instances
/// (revenue, distribution, payout, vault/account), four projection handlers,
/// and four GenericKafkaProjectionConsumerWorker instances (one per topic /
/// consumer group).
/// </summary>
public static class EconomicProjectionModule
{
    public static IServiceCollection AddEconomicProjection(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Projection stores ────────────────────────────────────
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<RevenueReadModel>("projection_economic_revenue_revenue", "revenue_read_model", "Revenue"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<DistributionReadModel>("projection_economic_revenue_distribution", "distribution_read_model", "Distribution"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<PayoutReadModel>("projection_economic_revenue_payout", "payout_read_model", "Payout"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<VaultAccountReadModel>("projection_economic_vault_account", "vault_account_read_model", "VaultAccount"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<EntryReadModel>("projection_economic_ledger_entry", "entry_read_model", "LedgerEntry"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<LedgerReadModel>("projection_economic_ledger_ledger", "ledger_read_model", "Ledger"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<ObligationReadModel>("projection_economic_ledger_obligation", "obligation_read_model", "Obligation"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<TreasuryReadModel>("projection_economic_ledger_treasury", "treasury_read_model", "Treasury"));

        // Capital projection stores (Phase 2 capital wiring — Option C)
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<CapitalAccountReadModel>("projection_economic_capital_account", "capital_account_read_model", "CapitalAccount"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<CapitalAllocationReadModel>("projection_economic_capital_allocation", "capital_allocation_read_model", "CapitalAllocation"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<CapitalAssetReadModel>("projection_economic_capital_asset", "capital_asset_read_model", "CapitalAsset"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<CapitalBindingReadModel>("projection_economic_capital_binding", "capital_binding_read_model", "CapitalBinding"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<CapitalPoolReadModel>("projection_economic_capital_pool", "capital_pool_read_model", "CapitalPool"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<CapitalReserveReadModel>("projection_economic_capital_reserve", "capital_reserve_read_model", "CapitalReserve"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<CapitalVaultReadModel>("projection_economic_capital_vault", "capital_vault_read_model", "CapitalVault"));

        // Compliance / audit projection store
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<AuditRecordReadModel>("projection_economic_compliance_audit", "audit_record_read_model", "AuditRecord"));

        // Enforcement projection stores (rule + violation + escalation)
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<EnforcementRuleReadModel>("projection_economic_enforcement_rule", "enforcement_rule_read_model", "EnforcementRule"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<ViolationReadModel>("projection_economic_enforcement_violation", "violation_read_model", "Violation"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<EscalationReadModel>("projection_economic_enforcement_escalation", "escalation_read_model", "ViolationEscalation"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<SanctionReadModel>("projection_economic_enforcement_sanction", "sanction_read_model", "Sanction"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<RestrictionReadModel>("projection_economic_enforcement_restriction", "restriction_read_model", "Restriction"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<LockReadModel>("projection_economic_enforcement_lock", "lock_read_model", "Lock"));

        // Risk projection store (exposure)
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<RiskExposureReadModel>("projection_economic_risk_exposure", "risk_exposure_read_model", "RiskExposure"));

        // Routing projection stores (path + execution)
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<RoutingPathReadModel>("projection_economic_routing_path", "routing_path_read_model", "RoutingPath"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<RoutingExecutionReadModel>("projection_economic_routing_execution", "routing_execution_read_model", "RoutingExecution"));

        // Subject projection store (economic subject bridge)
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<EconomicSubjectReadModel>("projection_economic_subject_subject", "economic_subject_read_model", "EconomicSubject"));

        // Exchange projection stores (fx + rate)
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<FxReadModel>("projection_economic_exchange_fx", "fx_read_model", "Fx"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<ExchangeRateReadModel>("projection_economic_exchange_rate", "exchange_rate_read_model", "ExchangeRate"));

        // Reconciliation projection stores (process + discrepancy)
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<ProcessReadModel>("projection_economic_reconciliation_process", "process_read_model", "ReconciliationProcess"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<DiscrepancyReadModel>("projection_economic_reconciliation_discrepancy", "discrepancy_read_model", "Discrepancy"));

        // Transaction projection stores (charge, expense, instruction, limit,
        // settlement, transaction, wallet).
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<ChargeReadModel>("projection_economic_transaction_charge", "charge_read_model", "Charge"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<ExpenseReadModel>("projection_economic_transaction_expense", "expense_read_model", "Expense"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<InstructionReadModel>("projection_economic_transaction_instruction", "instruction_read_model", "Instruction"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<LimitReadModel>("projection_economic_transaction_limit", "limit_read_model", "Limit"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<SettlementReadModel>("projection_economic_transaction_settlement", "settlement_read_model", "Settlement"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<TransactionReadModel>("projection_economic_transaction_transaction", "transaction_read_model", "Transaction"));
        services.AddSingleton(sp =>
            new ProjectionStoreFactory(sp.GetRequiredService<ProjectionsDataSource>().Inner)
                .Create<WalletReadModel>("projection_economic_transaction_wallet", "wallet_read_model", "Wallet"));

        // ── Projection handlers ──────────────────────────────────
        services.AddSingleton(sp => new RevenueRecordedProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<RevenueReadModel>>()));
        services.AddSingleton(sp => new DistributionCreatedProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<DistributionReadModel>>()));
        services.AddSingleton(sp => new PayoutExecutedProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<PayoutReadModel>>()));
        services.AddSingleton(sp => new VaultAccountProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<VaultAccountReadModel>>()));
        services.AddSingleton(sp => new EntryProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<EntryReadModel>>()));
        services.AddSingleton(sp => new LedgerUpdatedProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<LedgerReadModel>>()));
        services.AddSingleton(sp => new ObligationProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<ObligationReadModel>>()));
        services.AddSingleton(sp => new TreasuryProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<TreasuryReadModel>>()));

        // Capital projection handlers (Phase 2 capital wiring — Option C)
        services.AddSingleton(sp => new CapitalAccountProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<CapitalAccountReadModel>>()));
        services.AddSingleton(sp => new CapitalAllocationProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<CapitalAllocationReadModel>>()));
        services.AddSingleton(sp => new CapitalAssetProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<CapitalAssetReadModel>>()));
        services.AddSingleton(sp => new CapitalBindingProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<CapitalBindingReadModel>>()));
        services.AddSingleton(sp => new CapitalPoolProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<CapitalPoolReadModel>>()));
        services.AddSingleton(sp => new CapitalReserveProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<CapitalReserveReadModel>>()));
        services.AddSingleton(sp => new CapitalVaultProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<CapitalVaultReadModel>>()));

        services.AddSingleton(sp => new AuditRecordProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<AuditRecordReadModel>>()));

        services.AddSingleton(sp => new EnforcementRuleProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<EnforcementRuleReadModel>>()));
        services.AddSingleton(sp => new ViolationProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<ViolationReadModel>>()));
        services.AddSingleton(sp => new EscalationProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<EscalationReadModel>>()));
        services.AddSingleton(sp => new SanctionProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<SanctionReadModel>>()));
        services.AddSingleton(sp => new RestrictionProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<RestrictionReadModel>>()));
        services.AddSingleton(sp => new LockProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<LockReadModel>>()));

        // Risk projection handler (exposure)
        services.AddSingleton(sp => new RiskExposureProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<RiskExposureReadModel>>()));

        // Routing projection handlers (path + execution)
        services.AddSingleton(sp => new RoutingPathProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<RoutingPathReadModel>>()));
        services.AddSingleton(sp => new RoutingExecutionProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<RoutingExecutionReadModel>>()));

        // Subject projection handler
        services.AddSingleton(sp => new SubjectProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<EconomicSubjectReadModel>>()));

        // Exchange projection handlers (fx + rate)
        services.AddSingleton(sp => new FxProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<FxReadModel>>()));
        services.AddSingleton(sp => new ExchangeRateProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<ExchangeRateReadModel>>()));

        // Reconciliation projection handlers (process + discrepancy)
        services.AddSingleton(sp => new ProcessProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<ProcessReadModel>>()));
        services.AddSingleton(sp => new DiscrepancyProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<DiscrepancyReadModel>>()));

        // Transaction projection handlers.
        services.AddSingleton(sp => new ChargeProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<ChargeReadModel>>()));
        services.AddSingleton(sp => new ExpenseProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<ExpenseReadModel>>()));
        services.AddSingleton(sp => new InstructionProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<InstructionReadModel>>()));
        services.AddSingleton(sp => new LimitProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<LimitReadModel>>()));
        services.AddSingleton(sp => new SettlementProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<SettlementReadModel>>()));
        services.AddSingleton(sp => new TransactionProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<TransactionReadModel>>()));
        services.AddSingleton(sp => new WalletProjectionHandler(
            sp.GetRequiredService<PostgresProjectionStore<WalletReadModel>>()));

        // ── Kafka projection consumers (one per topic) ───────────
        var kafkaBootstrapServers = configuration.GetValue<string>("Kafka:BootstrapServers")
            ?? throw new InvalidOperationException("Kafka:BootstrapServers is required. No fallback.");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.revenue.revenue.events",
            consumerGroup: "whyce.projection.economic.revenue.revenue",
            projectionSchema: "projection_economic_revenue_revenue",
            projectionTable: "revenue_read_model",
            aggregateType: "Revenue");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.revenue.distribution.events",
            consumerGroup: "whyce.projection.economic.revenue.distribution",
            projectionSchema: "projection_economic_revenue_distribution",
            projectionTable: "distribution_read_model",
            aggregateType: "Distribution");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.revenue.payout.events",
            consumerGroup: "whyce.projection.economic.revenue.payout",
            projectionSchema: "projection_economic_revenue_payout",
            projectionTable: "payout_read_model",
            aggregateType: "Payout");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.vault.account.events",
            consumerGroup: "whyce.projection.economic.vault.account",
            projectionSchema: "projection_economic_vault_account",
            projectionTable: "vault_account_read_model",
            aggregateType: "VaultAccount");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.ledger.entry.events",
            consumerGroup: "whyce.projection.economic.ledger.entry",
            projectionSchema: "projection_economic_ledger_entry",
            projectionTable: "entry_read_model",
            aggregateType: "LedgerEntry");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.ledger.journal.events",
            consumerGroup: "whyce.projection.economic.ledger.journal",
            projectionSchema: "projection_economic_ledger_ledger",
            projectionTable: "ledger_read_model",
            aggregateType: "Ledger");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.ledger.ledger.events",
            consumerGroup: "whyce.projection.economic.ledger.ledger",
            projectionSchema: "projection_economic_ledger_ledger",
            projectionTable: "ledger_read_model",
            aggregateType: "Ledger");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.ledger.obligation.events",
            consumerGroup: "whyce.projection.economic.ledger.obligation",
            projectionSchema: "projection_economic_ledger_obligation",
            projectionTable: "obligation_read_model",
            aggregateType: "Obligation");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.ledger.treasury.events",
            consumerGroup: "whyce.projection.economic.ledger.treasury",
            projectionSchema: "projection_economic_ledger_treasury",
            projectionTable: "treasury_read_model",
            aggregateType: "Treasury");

        // Capital projection consumers (Phase 2 capital wiring — Option C)
        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.capital.account.events",
            consumerGroup: "whyce.projection.economic.capital.account",
            projectionSchema: "projection_economic_capital_account",
            projectionTable: "capital_account_read_model",
            aggregateType: "CapitalAccount");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.capital.allocation.events",
            consumerGroup: "whyce.projection.economic.capital.allocation",
            projectionSchema: "projection_economic_capital_allocation",
            projectionTable: "capital_allocation_read_model",
            aggregateType: "CapitalAllocation");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.capital.asset.events",
            consumerGroup: "whyce.projection.economic.capital.asset",
            projectionSchema: "projection_economic_capital_asset",
            projectionTable: "capital_asset_read_model",
            aggregateType: "CapitalAsset");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.capital.binding.events",
            consumerGroup: "whyce.projection.economic.capital.binding",
            projectionSchema: "projection_economic_capital_binding",
            projectionTable: "capital_binding_read_model",
            aggregateType: "CapitalBinding");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.capital.pool.events",
            consumerGroup: "whyce.projection.economic.capital.pool",
            projectionSchema: "projection_economic_capital_pool",
            projectionTable: "capital_pool_read_model",
            aggregateType: "CapitalPool");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.capital.reserve.events",
            consumerGroup: "whyce.projection.economic.capital.reserve",
            projectionSchema: "projection_economic_capital_reserve",
            projectionTable: "capital_reserve_read_model",
            aggregateType: "CapitalReserve");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.capital.vault.events",
            consumerGroup: "whyce.projection.economic.capital.vault",
            projectionSchema: "projection_economic_capital_vault",
            projectionTable: "capital_vault_read_model",
            aggregateType: "CapitalVault");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.compliance.audit.events",
            consumerGroup: "whyce.projection.economic.compliance.audit",
            projectionSchema: "projection_economic_compliance_audit",
            projectionTable: "audit_record_read_model",
            aggregateType: "AuditRecord");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.enforcement.rule.events",
            consumerGroup: "whyce.projection.economic.enforcement.rule",
            projectionSchema: "projection_economic_enforcement_rule",
            projectionTable: "enforcement_rule_read_model",
            aggregateType: "EnforcementRule");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.enforcement.violation.events",
            consumerGroup: "whyce.projection.economic.enforcement.violation",
            projectionSchema: "projection_economic_enforcement_violation",
            projectionTable: "violation_read_model",
            aggregateType: "Violation");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.enforcement.escalation.events",
            consumerGroup: "whyce.projection.economic.enforcement.escalation",
            projectionSchema: "projection_economic_enforcement_escalation",
            projectionTable: "escalation_read_model",
            aggregateType: "ViolationEscalation");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.enforcement.sanction.events",
            consumerGroup: "whyce.projection.economic.enforcement.sanction",
            projectionSchema: "projection_economic_enforcement_sanction",
            projectionTable: "sanction_read_model",
            aggregateType: "Sanction");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.enforcement.restriction.events",
            consumerGroup: "whyce.projection.economic.enforcement.restriction",
            projectionSchema: "projection_economic_enforcement_restriction",
            projectionTable: "restriction_read_model",
            aggregateType: "Restriction");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.enforcement.lock.events",
            consumerGroup: "whyce.projection.economic.enforcement.lock",
            projectionSchema: "projection_economic_enforcement_lock",
            projectionTable: "lock_read_model",
            aggregateType: "Lock");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.risk.exposure.events",
            consumerGroup: "whyce.projection.economic.risk.exposure",
            projectionSchema: "projection_economic_risk_exposure",
            projectionTable: "risk_exposure_read_model",
            aggregateType: "RiskExposure");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.routing.path.events",
            consumerGroup: "whyce.projection.economic.routing.path",
            projectionSchema: "projection_economic_routing_path",
            projectionTable: "routing_path_read_model",
            aggregateType: "RoutingPath");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.routing.execution.events",
            consumerGroup: "whyce.projection.economic.routing.execution",
            projectionSchema: "projection_economic_routing_execution",
            projectionTable: "routing_execution_read_model",
            aggregateType: "RoutingExecution");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.subject.subject.events",
            consumerGroup: "whyce.projection.economic.subject.subject",
            projectionSchema: "projection_economic_subject_subject",
            projectionTable: "economic_subject_read_model",
            aggregateType: "EconomicSubject");

        // Exchange projection consumers (fx + rate)
        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.exchange.fx.events",
            consumerGroup: "whyce.projection.economic.exchange.fx",
            projectionSchema: "projection_economic_exchange_fx",
            projectionTable: "fx_read_model",
            aggregateType: "Fx");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.exchange.rate.events",
            consumerGroup: "whyce.projection.economic.exchange.rate",
            projectionSchema: "projection_economic_exchange_rate",
            projectionTable: "exchange_rate_read_model",
            aggregateType: "ExchangeRate");

        // Transaction projection consumers.
        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.transaction.charge.events",
            consumerGroup: "whyce.projection.economic.transaction.charge",
            projectionSchema: "projection_economic_transaction_charge",
            projectionTable: "charge_read_model",
            aggregateType: "Charge");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.transaction.expense.events",
            consumerGroup: "whyce.projection.economic.transaction.expense",
            projectionSchema: "projection_economic_transaction_expense",
            projectionTable: "expense_read_model",
            aggregateType: "Expense");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.transaction.instruction.events",
            consumerGroup: "whyce.projection.economic.transaction.instruction",
            projectionSchema: "projection_economic_transaction_instruction",
            projectionTable: "instruction_read_model",
            aggregateType: "Instruction");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.transaction.limit.events",
            consumerGroup: "whyce.projection.economic.transaction.limit",
            projectionSchema: "projection_economic_transaction_limit",
            projectionTable: "limit_read_model",
            aggregateType: "Limit");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.transaction.settlement.events",
            consumerGroup: "whyce.projection.economic.transaction.settlement",
            projectionSchema: "projection_economic_transaction_settlement",
            projectionTable: "settlement_read_model",
            aggregateType: "Settlement");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.transaction.transaction.events",
            consumerGroup: "whyce.projection.economic.transaction.transaction",
            projectionSchema: "projection_economic_transaction_transaction",
            projectionTable: "transaction_read_model",
            aggregateType: "Transaction");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.transaction.wallet.events",
            consumerGroup: "whyce.projection.economic.transaction.wallet",
            projectionSchema: "projection_economic_transaction_wallet",
            projectionTable: "wallet_read_model",
            aggregateType: "Wallet");

        // Reconciliation projection consumers (process + discrepancy)
        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.reconciliation.process.events",
            consumerGroup: "whyce.projection.economic.reconciliation.process",
            projectionSchema: "projection_economic_reconciliation_process",
            projectionTable: "process_read_model",
            aggregateType: "ReconciliationProcess");

        RegisterWorker(services, kafkaBootstrapServers,
            topic: "whyce.economic.reconciliation.discrepancy.events",
            consumerGroup: "whyce.projection.economic.reconciliation.discrepancy",
            projectionSchema: "projection_economic_reconciliation_discrepancy",
            projectionTable: "discrepancy_read_model",
            aggregateType: "Discrepancy");

        return services;
    }

    public static void RegisterProjections(IServiceProvider provider, ProjectionRegistry projection)
    {
        var revenueHandler = provider.GetRequiredService<RevenueRecordedProjectionHandler>();
        projection.Register("RevenueRecordedEvent", revenueHandler);

        var distributionHandler = provider.GetRequiredService<DistributionCreatedProjectionHandler>();
        projection.Register("DistributionCreatedEvent", distributionHandler);

        var payoutHandler = provider.GetRequiredService<PayoutExecutedProjectionHandler>();
        projection.Register("PayoutExecutedEvent", payoutHandler);

        var vaultHandler = provider.GetRequiredService<VaultAccountProjectionHandler>();
        projection.Register("VaultFundedEvent", vaultHandler);
        projection.Register("CapitalAllocatedToSliceEvent", vaultHandler);
        projection.Register("SpvProfitReceivedEvent", vaultHandler);
        projection.Register("VaultDebitedEvent", vaultHandler);
        projection.Register("VaultCreditedEvent", vaultHandler);

        var entryHandler = provider.GetRequiredService<EntryProjectionHandler>();
        projection.Register("LedgerEntryRecordedEvent", entryHandler);

        var ledgerHandler = provider.GetRequiredService<LedgerUpdatedProjectionHandler>();
        projection.Register("LedgerOpenedEvent", ledgerHandler);
        projection.Register("LedgerUpdatedEvent", ledgerHandler);
        projection.Register("JournalEntryAddedEvent", ledgerHandler);

        var obligationHandler = provider.GetRequiredService<ObligationProjectionHandler>();
        projection.Register("ObligationCreatedEvent", obligationHandler);
        projection.Register("ObligationFulfilledEvent", obligationHandler);
        projection.Register("ObligationCancelledEvent", obligationHandler);

        var treasuryHandler = provider.GetRequiredService<TreasuryProjectionHandler>();
        projection.Register("TreasuryCreatedEvent", treasuryHandler);
        projection.Register("TreasuryFundAllocatedEvent", treasuryHandler);
        projection.Register("TreasuryFundReleasedEvent", treasuryHandler);

        // Capital projection registrations (Phase 2 capital wiring — Option C)
        var capitalAccountHandler = provider.GetRequiredService<CapitalAccountProjectionHandler>();
        projection.Register("CapitalAccountOpenedEvent", capitalAccountHandler);
        projection.Register("CapitalFundedEvent", capitalAccountHandler);
        projection.Register("AccountCapitalAllocatedEvent", capitalAccountHandler);
        projection.Register("AccountCapitalReservedEvent", capitalAccountHandler);
        projection.Register("AccountReservationReleasedEvent", capitalAccountHandler);
        projection.Register("CapitalAccountFrozenEvent", capitalAccountHandler);
        projection.Register("CapitalAccountClosedEvent", capitalAccountHandler);

        var capitalAllocationHandler = provider.GetRequiredService<CapitalAllocationProjectionHandler>();
        projection.Register("AllocationCreatedEvent", capitalAllocationHandler);
        projection.Register("AllocationReleasedEvent", capitalAllocationHandler);
        projection.Register("AllocationCompletedEvent", capitalAllocationHandler);
        projection.Register("CapitalAllocatedToSpvEvent", capitalAllocationHandler);

        var capitalAssetHandler = provider.GetRequiredService<CapitalAssetProjectionHandler>();
        projection.Register("AssetCreatedEvent", capitalAssetHandler);
        projection.Register("AssetValuedEvent", capitalAssetHandler);
        projection.Register("AssetDisposedEvent", capitalAssetHandler);

        var capitalBindingHandler = provider.GetRequiredService<CapitalBindingProjectionHandler>();
        projection.Register("CapitalBoundEvent", capitalBindingHandler);
        projection.Register("OwnershipTransferredEvent", capitalBindingHandler);
        projection.Register("BindingReleasedEvent", capitalBindingHandler);

        var capitalPoolHandler = provider.GetRequiredService<CapitalPoolProjectionHandler>();
        projection.Register("PoolCreatedEvent", capitalPoolHandler);
        projection.Register("CapitalAggregatedEvent", capitalPoolHandler);
        projection.Register("CapitalReducedEvent", capitalPoolHandler);

        var capitalReserveHandler = provider.GetRequiredService<CapitalReserveProjectionHandler>();
        projection.Register("ReserveCreatedEvent", capitalReserveHandler);
        projection.Register("ReserveReleasedEvent", capitalReserveHandler);
        projection.Register("ReserveExpiredEvent", capitalReserveHandler);

        var capitalVaultHandler = provider.GetRequiredService<CapitalVaultProjectionHandler>();
        projection.Register("VaultCreatedEvent", capitalVaultHandler);
        projection.Register("VaultSliceCreatedEvent", capitalVaultHandler);
        projection.Register("CapitalDepositedEvent", capitalVaultHandler);
        projection.Register("CapitalAllocatedToSliceEvent", capitalVaultHandler);
        projection.Register("CapitalReleasedFromSliceEvent", capitalVaultHandler);
        projection.Register("CapitalWithdrawnEvent", capitalVaultHandler);

        var auditRecordHandler = provider.GetRequiredService<AuditRecordProjectionHandler>();
        projection.Register("AuditRecordCreatedEvent", auditRecordHandler);
        projection.Register("AuditRecordFinalizedEvent", auditRecordHandler);

        var enforcementRuleHandler = provider.GetRequiredService<EnforcementRuleProjectionHandler>();
        projection.Register("EnforcementRuleDefinedEvent", enforcementRuleHandler);
        projection.Register("EnforcementRuleActivatedEvent", enforcementRuleHandler);
        projection.Register("EnforcementRuleDisabledEvent", enforcementRuleHandler);
        projection.Register("EnforcementRuleRetiredEvent", enforcementRuleHandler);

        var violationHandler = provider.GetRequiredService<ViolationProjectionHandler>();
        projection.Register("ViolationDetectedEvent", violationHandler);
        projection.Register("ViolationAcknowledgedEvent", violationHandler);
        projection.Register("ViolationResolvedEvent", violationHandler);

        var sanctionHandler = provider.GetRequiredService<SanctionProjectionHandler>();
        projection.Register("SanctionIssuedEvent", sanctionHandler);
        projection.Register("SanctionActivatedEvent", sanctionHandler);
        projection.Register("SanctionExpiredEvent", sanctionHandler);
        projection.Register("SanctionRevokedEvent", sanctionHandler);

        var restrictionHandler = provider.GetRequiredService<RestrictionProjectionHandler>();
        projection.Register("RestrictionAppliedEvent", restrictionHandler);
        projection.Register("RestrictionUpdatedEvent", restrictionHandler);
        projection.Register("RestrictionRemovedEvent", restrictionHandler);

        var lockHandler = provider.GetRequiredService<LockProjectionHandler>();
        projection.Register("SystemLockedEvent", lockHandler);
        projection.Register("SystemUnlockedEvent", lockHandler);

        // Risk projection registrations (exposure)
        var riskExposureHandler = provider.GetRequiredService<RiskExposureProjectionHandler>();
        projection.Register("RiskExposureCreatedEvent", riskExposureHandler);
        projection.Register("RiskExposureIncreasedEvent", riskExposureHandler);
        projection.Register("RiskExposureReducedEvent", riskExposureHandler);
        projection.Register("RiskExposureClosedEvent", riskExposureHandler);

        // Routing projection registrations (path + execution)
        var routingPathHandler = provider.GetRequiredService<RoutingPathProjectionHandler>();
        projection.Register("RoutingPathDefinedEvent", routingPathHandler);
        projection.Register("RoutingPathActivatedEvent", routingPathHandler);
        projection.Register("RoutingPathDisabledEvent", routingPathHandler);

        var routingExecutionHandler = provider.GetRequiredService<RoutingExecutionProjectionHandler>();
        projection.Register("ExecutionStartedEvent", routingExecutionHandler);
        projection.Register("ExecutionCompletedEvent", routingExecutionHandler);
        projection.Register("ExecutionFailedEvent", routingExecutionHandler);
        projection.Register("ExecutionAbortedEvent", routingExecutionHandler);

        // Subject projection registration
        var subjectHandler = provider.GetRequiredService<SubjectProjectionHandler>();
        projection.Register("EconomicSubjectRegisteredEvent", subjectHandler);

        // Exchange projection registrations (fx + rate)
        var fxHandler = provider.GetRequiredService<FxProjectionHandler>();
        projection.Register("FxPairRegisteredEvent", fxHandler);
        projection.Register("FxPairActivatedEvent", fxHandler);
        projection.Register("FxPairDeactivatedEvent", fxHandler);

        var exchangeRateHandler = provider.GetRequiredService<ExchangeRateProjectionHandler>();
        projection.Register("ExchangeRateDefinedEvent", exchangeRateHandler);
        projection.Register("ExchangeRateActivatedEvent", exchangeRateHandler);
        projection.Register("ExchangeRateExpiredEvent", exchangeRateHandler);

        // Transaction projection registrations (charge, expense, instruction,
        // limit, settlement, transaction, wallet).
        var chargeHandler = provider.GetRequiredService<ChargeProjectionHandler>();
        projection.Register("ChargeCalculatedEvent", chargeHandler);
        projection.Register("ChargeAppliedEvent", chargeHandler);

        var expenseHandler = provider.GetRequiredService<ExpenseProjectionHandler>();
        projection.Register("ExpenseCreatedEvent", expenseHandler);
        projection.Register("ExpenseRecordedEvent", expenseHandler);
        projection.Register("ExpenseCancelledEvent", expenseHandler);

        var instructionHandler = provider.GetRequiredService<InstructionProjectionHandler>();
        projection.Register("TransactionInstructionCreatedEvent", instructionHandler);
        projection.Register("TransactionInstructionExecutedEvent", instructionHandler);
        projection.Register("TransactionInstructionCancelledEvent", instructionHandler);

        var limitHandler = provider.GetRequiredService<LimitProjectionHandler>();
        projection.Register("LimitDefinedEvent", limitHandler);
        projection.Register("LimitCheckedEvent", limitHandler);
        projection.Register("LimitExceededEvent", limitHandler);

        var settlementHandler = provider.GetRequiredService<SettlementProjectionHandler>();
        projection.Register("SettlementInitiatedEvent", settlementHandler);
        projection.Register("SettlementProcessingStartedEvent", settlementHandler);
        projection.Register("SettlementCompletedEvent", settlementHandler);
        projection.Register("SettlementFailedEvent", settlementHandler);

        var transactionEnvelopeHandler = provider.GetRequiredService<TransactionProjectionHandler>();
        projection.Register("TransactionInitiatedEvent", transactionEnvelopeHandler);
        projection.Register("TransactionProcessingStartedEvent", transactionEnvelopeHandler);
        projection.Register("TransactionCommittedEvent", transactionEnvelopeHandler);
        projection.Register("TransactionFailedEvent", transactionEnvelopeHandler);

        var walletHandler = provider.GetRequiredService<WalletProjectionHandler>();
        projection.Register("WalletCreatedEvent", walletHandler);
        projection.Register("TransactionRequestedEvent", walletHandler);

        // Reconciliation projection registrations (process + discrepancy)
        var reconciliationProcessHandler = provider.GetRequiredService<ProcessProjectionHandler>();
        projection.Register("ReconciliationTriggeredEvent",  reconciliationProcessHandler);
        projection.Register("ReconciliationMatchedEvent",    reconciliationProcessHandler);
        projection.Register("ReconciliationMismatchedEvent", reconciliationProcessHandler);
        projection.Register("ReconciliationResolvedEvent",   reconciliationProcessHandler);

        var discrepancyHandler = provider.GetRequiredService<DiscrepancyProjectionHandler>();
        projection.Register("DiscrepancyDetectedEvent",     discrepancyHandler);
        projection.Register("DiscrepancyInvestigatedEvent", discrepancyHandler);
        projection.Register("DiscrepancyResolvedEvent",     discrepancyHandler);
    }

    private static void RegisterWorker(
        IServiceCollection services,
        string kafkaBootstrapServers,
        string topic,
        string consumerGroup,
        string projectionSchema,
        string projectionTable,
        string aggregateType)
    {
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
                sp.GetRequiredService<Whycespace.Shared.Contracts.Infrastructure.Messaging.KafkaConsumerOptions>(),
                sp.GetRequiredService<Whycespace.Shared.Contracts.Infrastructure.Health.IWorkerLivenessRegistry>(),
                sp.GetService<Microsoft.Extensions.Logging.ILogger<GenericKafkaProjectionConsumerWorker>>()));
    }
}
