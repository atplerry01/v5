namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// R2.E.4 / R-TOPIC-CANONICAL-DECLARATION-01 — central declaration of
/// the Kafka topics the runtime expects to exist on the broker.
///
/// MIRROR: <c>infrastructure/event-fabric/kafka/create-topics.sh</c>
/// is the OPERATOR-LEVEL source of truth for topic creation (including
/// partition count and replication factor). This class is the
/// RUNTIME-LEVEL source of truth for alignment verification at startup.
///
/// When a new bounded context ships, BOTH files must be updated:
///   1. <c>infrastructure/event-fabric/kafka/create-topics.sh</c> —
///      add the four-tier block (commands / events / retry / deadletter)
///      with the canonical partition / replication settings.
///   2. <c>KafkaCanonicalTopics.cs</c> (this file) — add the four
///      topic names to <see cref="All"/>.
///
/// The two files are a known drift surface; the post-R2.E.4 convention
/// is that a PR modifying one MUST modify the other. Future R3
/// tooling may auto-generate the bash script from this declaration
/// to remove the drift surface entirely; for R2.E.4 the manual mirror
/// is acceptable and alignment is verified at startup.
///
/// Topic tiers per bounded context (four channel types):
///   - <c>.commands</c>   — command-dispatch topic (inbound write requests)
///   - <c>.events</c>     — canonical domain event emission
///   - <c>.retry</c>      — R2.A.3a tiered retry topic (deliver-after)
///   - <c>.deadletter</c> — R2.A.3b terminal dead-letter topic
///
/// The R2.A.D.3b four-tier discipline is enforced by
/// <see cref="Whycespace.Runtime.EventFabric.TopicNameResolver"/>.
/// </summary>
public static class KafkaCanonicalTopics
{
    // Each nested array is one bounded context. Flattened into All below.
    // Order mirrors the blocks in create-topics.sh so a diff review is
    // obvious. Adding a new context: append a new static string[] and
    // add it to the All composition expression.

    private static readonly string[] EconomicLedgerTransaction =
    [
        "whyce.economic.ledger.transaction.commands",
        "whyce.economic.ledger.transaction.events",
        "whyce.economic.ledger.transaction.retry",
        "whyce.economic.ledger.transaction.deadletter",
    ];

    private static readonly string[] IdentityAccessIdentity =
    [
        "whyce.identity.access.identity.commands",
        "whyce.identity.access.identity.events",
        "whyce.identity.access.identity.retry",
        "whyce.identity.access.identity.deadletter",
    ];

    private static readonly string[] OperationalGlobalIncident =
    [
        "whyce.operational.global.incident.commands",
        "whyce.operational.global.incident.events",
        "whyce.operational.global.incident.retry",
        "whyce.operational.global.incident.deadletter",
    ];

    private static readonly string[] OperationalSandboxTodo =
    [
        "whyce.operational.sandbox.todo.commands",
        "whyce.operational.sandbox.todo.events",
        "whyce.operational.sandbox.todo.retry",
        "whyce.operational.sandbox.todo.deadletter",
    ];

    private static readonly string[] OperationalSandboxKanban =
    [
        "whyce.operational.sandbox.kanban.commands",
        "whyce.operational.sandbox.kanban.events",
        "whyce.operational.sandbox.kanban.retry",
        "whyce.operational.sandbox.kanban.deadletter",
    ];

    private static readonly string[] ConstitutionalPolicyDecision =
    [
        "whyce.constitutional.policy.decision.commands",
        "whyce.constitutional.policy.decision.events",
        "whyce.constitutional.policy.decision.retry",
        "whyce.constitutional.policy.decision.deadletter",
    ];

    // Policy feedback bridge has events + deadletter only (no commands,
    // no retry). Mirrors create-topics.sh line 237-239.
    private static readonly string[] ConstitutionalPolicyFeedback =
    [
        "whyce.constitutional.policy.feedback.events",
        "whyce.constitutional.policy.feedback.deadletter",
    ];

    private static readonly string[] EconomicRevenueRevenue =
    [
        "whyce.economic.revenue.revenue.commands",
        "whyce.economic.revenue.revenue.events",
        "whyce.economic.revenue.revenue.retry",
        "whyce.economic.revenue.revenue.deadletter",
    ];

    private static readonly string[] EconomicRevenueDistribution =
    [
        "whyce.economic.revenue.distribution.commands",
        "whyce.economic.revenue.distribution.events",
        "whyce.economic.revenue.distribution.retry",
        "whyce.economic.revenue.distribution.deadletter",
    ];

    private static readonly string[] EconomicRevenuePayout =
    [
        "whyce.economic.revenue.payout.commands",
        "whyce.economic.revenue.payout.events",
        "whyce.economic.revenue.payout.retry",
        "whyce.economic.revenue.payout.deadletter",
    ];

    private static readonly string[] EconomicRevenueContract =
    [
        "whyce.economic.revenue.contract.commands",
        "whyce.economic.revenue.contract.events",
        "whyce.economic.revenue.contract.retry",
        "whyce.economic.revenue.contract.deadletter",
    ];

    private static readonly string[] EconomicRevenuePricing =
    [
        "whyce.economic.revenue.pricing.commands",
        "whyce.economic.revenue.pricing.events",
        "whyce.economic.revenue.pricing.retry",
        "whyce.economic.revenue.pricing.deadletter",
    ];

    private static readonly string[] EconomicVaultAccount =
    [
        "whyce.economic.vault.account.commands",
        "whyce.economic.vault.account.events",
        "whyce.economic.vault.account.retry",
        "whyce.economic.vault.account.deadletter",
    ];

    private static readonly string[] EconomicTransactionExpense =
    [
        "whyce.economic.transaction.expense.commands",
        "whyce.economic.transaction.expense.events",
        "whyce.economic.transaction.expense.retry",
        "whyce.economic.transaction.expense.deadletter",
    ];

    private static readonly string[] EconomicLedgerJournal =
    [
        "whyce.economic.ledger.journal.commands",
        "whyce.economic.ledger.journal.events",
        "whyce.economic.ledger.journal.retry",
        "whyce.economic.ledger.journal.deadletter",
    ];

    private static readonly string[] EconomicLedgerLedger =
    [
        "whyce.economic.ledger.ledger.commands",
        "whyce.economic.ledger.ledger.events",
        "whyce.economic.ledger.ledger.retry",
        "whyce.economic.ledger.ledger.deadletter",
    ];

    private static readonly string[] EconomicLedgerEntry =
    [
        "whyce.economic.ledger.entry.commands",
        "whyce.economic.ledger.entry.events",
        "whyce.economic.ledger.entry.retry",
        "whyce.economic.ledger.entry.deadletter",
    ];

    private static readonly string[] EconomicLedgerObligation =
    [
        "whyce.economic.ledger.obligation.commands",
        "whyce.economic.ledger.obligation.events",
        "whyce.economic.ledger.obligation.retry",
        "whyce.economic.ledger.obligation.deadletter",
    ];

    private static readonly string[] EconomicLedgerTreasury =
    [
        "whyce.economic.ledger.treasury.commands",
        "whyce.economic.ledger.treasury.events",
        "whyce.economic.ledger.treasury.retry",
        "whyce.economic.ledger.treasury.deadletter",
    ];

    private static readonly string[] EconomicTransactionSettlement =
    [
        "whyce.economic.transaction.settlement.commands",
        "whyce.economic.transaction.settlement.events",
        "whyce.economic.transaction.settlement.retry",
        "whyce.economic.transaction.settlement.deadletter",
    ];

    private static readonly string[] EconomicTransactionTransaction =
    [
        "whyce.economic.transaction.transaction.commands",
        "whyce.economic.transaction.transaction.events",
        "whyce.economic.transaction.transaction.retry",
        "whyce.economic.transaction.transaction.deadletter",
    ];

    private static readonly string[] EconomicTransactionCharge =
    [
        "whyce.economic.transaction.charge.commands",
        "whyce.economic.transaction.charge.events",
        "whyce.economic.transaction.charge.retry",
        "whyce.economic.transaction.charge.deadletter",
    ];

    private static readonly string[] EconomicTransactionInstruction =
    [
        "whyce.economic.transaction.instruction.commands",
        "whyce.economic.transaction.instruction.events",
        "whyce.economic.transaction.instruction.retry",
        "whyce.economic.transaction.instruction.deadletter",
    ];

    private static readonly string[] EconomicTransactionLimit =
    [
        "whyce.economic.transaction.limit.commands",
        "whyce.economic.transaction.limit.events",
        "whyce.economic.transaction.limit.retry",
        "whyce.economic.transaction.limit.deadletter",
    ];

    private static readonly string[] EconomicTransactionWallet =
    [
        "whyce.economic.transaction.wallet.commands",
        "whyce.economic.transaction.wallet.events",
        "whyce.economic.transaction.wallet.retry",
        "whyce.economic.transaction.wallet.deadletter",
    ];

    private static readonly string[] EconomicCapitalAccount =
    [
        "whyce.economic.capital.account.commands",
        "whyce.economic.capital.account.events",
        "whyce.economic.capital.account.retry",
        "whyce.economic.capital.account.deadletter",
    ];

    private static readonly string[] EconomicCapitalAllocation =
    [
        "whyce.economic.capital.allocation.commands",
        "whyce.economic.capital.allocation.events",
        "whyce.economic.capital.allocation.retry",
        "whyce.economic.capital.allocation.deadletter",
    ];

    private static readonly string[] EconomicCapitalAsset =
    [
        "whyce.economic.capital.asset.commands",
        "whyce.economic.capital.asset.events",
        "whyce.economic.capital.asset.retry",
        "whyce.economic.capital.asset.deadletter",
    ];

    private static readonly string[] EconomicCapitalBinding =
    [
        "whyce.economic.capital.binding.commands",
        "whyce.economic.capital.binding.events",
        "whyce.economic.capital.binding.retry",
        "whyce.economic.capital.binding.deadletter",
    ];

    private static readonly string[] EconomicCapitalPool =
    [
        "whyce.economic.capital.pool.commands",
        "whyce.economic.capital.pool.events",
        "whyce.economic.capital.pool.retry",
        "whyce.economic.capital.pool.deadletter",
    ];

    private static readonly string[] EconomicCapitalReserve =
    [
        "whyce.economic.capital.reserve.commands",
        "whyce.economic.capital.reserve.events",
        "whyce.economic.capital.reserve.retry",
        "whyce.economic.capital.reserve.deadletter",
    ];

    private static readonly string[] EconomicCapitalVault =
    [
        "whyce.economic.capital.vault.commands",
        "whyce.economic.capital.vault.events",
        "whyce.economic.capital.vault.retry",
        "whyce.economic.capital.vault.deadletter",
    ];

    private static readonly string[] EconomicEnforcementRule =
    [
        "whyce.economic.enforcement.rule.commands",
        "whyce.economic.enforcement.rule.events",
        "whyce.economic.enforcement.rule.retry",
        "whyce.economic.enforcement.rule.deadletter",
    ];

    private static readonly string[] EconomicEnforcementViolation =
    [
        "whyce.economic.enforcement.violation.commands",
        "whyce.economic.enforcement.violation.events",
        "whyce.economic.enforcement.violation.retry",
        "whyce.economic.enforcement.violation.deadletter",
    ];

    private static readonly string[] EconomicEnforcementEscalation =
    [
        "whyce.economic.enforcement.escalation.commands",
        "whyce.economic.enforcement.escalation.events",
        "whyce.economic.enforcement.escalation.retry",
        "whyce.economic.enforcement.escalation.deadletter",
    ];

    private static readonly string[] EconomicEnforcementLock =
    [
        "whyce.economic.enforcement.lock.commands",
        "whyce.economic.enforcement.lock.events",
        "whyce.economic.enforcement.lock.retry",
        "whyce.economic.enforcement.lock.deadletter",
    ];

    private static readonly string[] EconomicEnforcementRestriction =
    [
        "whyce.economic.enforcement.restriction.commands",
        "whyce.economic.enforcement.restriction.events",
        "whyce.economic.enforcement.restriction.retry",
        "whyce.economic.enforcement.restriction.deadletter",
    ];

    private static readonly string[] EconomicEnforcementSanction =
    [
        "whyce.economic.enforcement.sanction.commands",
        "whyce.economic.enforcement.sanction.events",
        "whyce.economic.enforcement.sanction.retry",
        "whyce.economic.enforcement.sanction.deadletter",
    ];

    private static readonly string[] EconomicExchangeFx =
    [
        "whyce.economic.exchange.fx.commands",
        "whyce.economic.exchange.fx.events",
        "whyce.economic.exchange.fx.retry",
        "whyce.economic.exchange.fx.deadletter",
    ];

    private static readonly string[] EconomicExchangeRate =
    [
        "whyce.economic.exchange.rate.commands",
        "whyce.economic.exchange.rate.events",
        "whyce.economic.exchange.rate.retry",
        "whyce.economic.exchange.rate.deadletter",
    ];

    private static readonly string[] EconomicReconciliationProcess =
    [
        "whyce.economic.reconciliation.process.commands",
        "whyce.economic.reconciliation.process.events",
        "whyce.economic.reconciliation.process.retry",
        "whyce.economic.reconciliation.process.deadletter",
    ];

    private static readonly string[] EconomicReconciliationDiscrepancy =
    [
        "whyce.economic.reconciliation.discrepancy.commands",
        "whyce.economic.reconciliation.discrepancy.events",
        "whyce.economic.reconciliation.discrepancy.retry",
        "whyce.economic.reconciliation.discrepancy.deadletter",
    ];

    private static readonly string[] EconomicRoutingPath =
    [
        "whyce.economic.routing.path.commands",
        "whyce.economic.routing.path.events",
        "whyce.economic.routing.path.retry",
        "whyce.economic.routing.path.deadletter",
    ];

    private static readonly string[] EconomicRoutingExecution =
    [
        "whyce.economic.routing.execution.commands",
        "whyce.economic.routing.execution.events",
        "whyce.economic.routing.execution.retry",
        "whyce.economic.routing.execution.deadletter",
    ];

    private static readonly string[] EconomicSubjectSubject =
    [
        "whyce.economic.subject.subject.commands",
        "whyce.economic.subject.subject.events",
        "whyce.economic.subject.subject.retry",
        "whyce.economic.subject.subject.deadletter",
    ];

    private static readonly string[] EconomicComplianceAudit =
    [
        "whyce.economic.compliance.audit.commands",
        "whyce.economic.compliance.audit.events",
        "whyce.economic.compliance.audit.retry",
        "whyce.economic.compliance.audit.deadletter",
    ];

    private static readonly string[] EconomicRiskExposure =
    [
        "whyce.economic.risk.exposure.commands",
        "whyce.economic.risk.exposure.events",
        "whyce.economic.risk.exposure.retry",
        "whyce.economic.risk.exposure.deadletter",
    ];

    /// <summary>
    /// Flat, de-duplicated, ordered view of every topic name the runtime
    /// expects to exist on the broker. Enumeration order is stable across
    /// process restarts (alphabetical by topic name) so the startup log
    /// is diff-comparable.
    /// </summary>
    public static readonly IReadOnlyList<string> All = new[]
    {
        EconomicLedgerTransaction,
        IdentityAccessIdentity,
        OperationalGlobalIncident,
        OperationalSandboxTodo,
        OperationalSandboxKanban,
        ConstitutionalPolicyDecision,
        ConstitutionalPolicyFeedback,
        EconomicRevenueRevenue,
        EconomicRevenueDistribution,
        EconomicRevenuePayout,
        EconomicRevenueContract,
        EconomicRevenuePricing,
        EconomicVaultAccount,
        EconomicTransactionExpense,
        EconomicLedgerJournal,
        EconomicLedgerLedger,
        EconomicLedgerEntry,
        EconomicLedgerObligation,
        EconomicLedgerTreasury,
        EconomicTransactionSettlement,
        EconomicTransactionTransaction,
        EconomicTransactionCharge,
        EconomicTransactionInstruction,
        EconomicTransactionLimit,
        EconomicTransactionWallet,
        EconomicCapitalAccount,
        EconomicCapitalAllocation,
        EconomicCapitalAsset,
        EconomicCapitalBinding,
        EconomicCapitalPool,
        EconomicCapitalReserve,
        EconomicCapitalVault,
        EconomicEnforcementRule,
        EconomicEnforcementViolation,
        EconomicEnforcementEscalation,
        EconomicEnforcementLock,
        EconomicEnforcementRestriction,
        EconomicEnforcementSanction,
        EconomicExchangeFx,
        EconomicExchangeRate,
        EconomicReconciliationProcess,
        EconomicReconciliationDiscrepancy,
        EconomicRoutingPath,
        EconomicRoutingExecution,
        EconomicSubjectSubject,
        EconomicComplianceAudit,
        EconomicRiskExposure,
    }
    .SelectMany(block => block)
    .Distinct(StringComparer.Ordinal)
    .OrderBy(t => t, StringComparer.Ordinal)
    .ToList()
    .AsReadOnly();
}
