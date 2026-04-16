using Whycespace.Shared.Contracts.Events.Economic.Ledger.Journal;
using Whycespace.Shared.Contracts.Events.Economic.Ledger.Ledger;
using LedgerEntrySchema = Whycespace.Shared.Contracts.Events.Economic.Ledger.Entry;
using LedgerEntryDomain = Whycespace.Domain.EconomicSystem.Ledger.Entry;
using LedgerObligationSchema = Whycespace.Shared.Contracts.Events.Economic.Ledger.Obligation;
using LedgerTreasurySchema = Whycespace.Shared.Contracts.Events.Economic.Ledger.Treasury;
using LedgerObligationDomain = Whycespace.Domain.EconomicSystem.Ledger.Obligation;
using LedgerTreasuryDomain = Whycespace.Domain.EconomicSystem.Ledger.Treasury;
using Whycespace.Shared.Contracts.Events.Economic.Revenue.Distribution;
using Whycespace.Shared.Contracts.Events.Economic.Revenue.Payout;
using Whycespace.Shared.Contracts.Events.Economic.Revenue.Revenue;
using Whycespace.Shared.Contracts.Events.Economic.Vault.Account;
using CapitalAccountSchema = Whycespace.Shared.Contracts.Events.Economic.Capital.Account;
using CapitalAllocationSchema = Whycespace.Shared.Contracts.Events.Economic.Capital.Allocation;
using CapitalAssetSchema = Whycespace.Shared.Contracts.Events.Economic.Capital.Asset;
using CapitalBindingSchema = Whycespace.Shared.Contracts.Events.Economic.Capital.Binding;
using CapitalPoolSchema = Whycespace.Shared.Contracts.Events.Economic.Capital.Pool;
using CapitalReserveSchema = Whycespace.Shared.Contracts.Events.Economic.Capital.Reserve;
using CapitalVaultSchema = Whycespace.Shared.Contracts.Events.Economic.Capital.Vault;
using ContractSlice = Whycespace.Shared.Contracts.Events.Economic.Vault.Account;
using JournalDomain = Whycespace.Domain.EconomicSystem.Ledger.Journal;
using LedgerDomain = Whycespace.Domain.EconomicSystem.Ledger.Ledger;
using RevenueDomain = Whycespace.Domain.EconomicSystem.Revenue.Revenue;
using DistributionDomain = Whycespace.Domain.EconomicSystem.Revenue.Distribution;
using PayoutDomain = Whycespace.Domain.EconomicSystem.Revenue.Payout;
using VaultDomain = Whycespace.Domain.EconomicSystem.Vault.Account;
using SliceDomain = Whycespace.Domain.EconomicSystem.Vault.Slice;
using CapitalAccountDomain = Whycespace.Domain.EconomicSystem.Capital.Account;
using CapitalAllocationDomain = Whycespace.Domain.EconomicSystem.Capital.Allocation;
using CapitalAssetDomain = Whycespace.Domain.EconomicSystem.Capital.Asset;
using CapitalBindingDomain = Whycespace.Domain.EconomicSystem.Capital.Binding;
using CapitalPoolDomain = Whycespace.Domain.EconomicSystem.Capital.Pool;
using CapitalReserveDomain = Whycespace.Domain.EconomicSystem.Capital.Reserve;
using CapitalVaultDomain = Whycespace.Domain.EconomicSystem.Capital.Vault;
using AuditDomain = Whycespace.Domain.EconomicSystem.Compliance.Audit;
using AuditSchema = Whycespace.Shared.Contracts.Events.Economic.Compliance.Audit;
using EnforcementRuleDomain = Whycespace.Domain.EconomicSystem.Enforcement.Rule;
using EnforcementRuleSchema = Whycespace.Shared.Contracts.Events.Economic.Enforcement.Rule;
using EnforcementViolationDomain = Whycespace.Domain.EconomicSystem.Enforcement.Violation;
using EnforcementViolationSchema = Whycespace.Shared.Contracts.Events.Economic.Enforcement.Violation;
using EnforcementEscalationDomain = Whycespace.Domain.EconomicSystem.Enforcement.Escalation;
using EnforcementEscalationSchema = Whycespace.Shared.Contracts.Events.Economic.Enforcement.Escalation;
using EnforcementSanctionDomain = Whycespace.Domain.EconomicSystem.Enforcement.Sanction;
using EnforcementSanctionSchema = Whycespace.Shared.Contracts.Events.Economic.Enforcement.Sanction;
using EnforcementRestrictionDomain = Whycespace.Domain.EconomicSystem.Enforcement.Restriction;
using EnforcementRestrictionSchema = Whycespace.Shared.Contracts.Events.Economic.Enforcement.Restriction;
using EnforcementLockDomain = Whycespace.Domain.EconomicSystem.Enforcement.Lock;
using EnforcementLockSchema = Whycespace.Shared.Contracts.Events.Economic.Enforcement.Lock;
using RiskExposureDomain = Whycespace.Domain.EconomicSystem.Risk.Exposure;
using RiskExposureSchema = Whycespace.Shared.Contracts.Events.Economic.Risk.Exposure;
using RoutingPathDomain = Whycespace.Domain.EconomicSystem.Routing.Path;
using RoutingPathSchema = Whycespace.Shared.Contracts.Events.Economic.Routing.Path;
using RoutingExecutionDomain = Whycespace.Domain.EconomicSystem.Routing.Execution;
using RoutingExecutionSchema = Whycespace.Shared.Contracts.Events.Economic.Routing.Execution;
using SubjectDomain = Whycespace.Domain.EconomicSystem.Subject.Subject;
using SubjectSchema = Whycespace.Shared.Contracts.Events.Economic.Subject.Subject;
using ExchangeFxDomain = Whycespace.Domain.EconomicSystem.Exchange.Fx;
using ExchangeFxSchema = Whycespace.Shared.Contracts.Events.Economic.Exchange.Fx;
using ExchangeRateDomain = Whycespace.Domain.EconomicSystem.Exchange.Rate;
using ExchangeRateSchema = Whycespace.Shared.Contracts.Events.Economic.Exchange.Rate;
using ReconciliationProcessDomain = Whycespace.Domain.EconomicSystem.Reconciliation.Process;
using ReconciliationProcessSchema = Whycespace.Shared.Contracts.Events.Economic.Reconciliation.Process;
using ReconciliationDiscrepancyDomain = Whycespace.Domain.EconomicSystem.Reconciliation.Discrepancy;
using ReconciliationDiscrepancySchema = Whycespace.Shared.Contracts.Events.Economic.Reconciliation.Discrepancy;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Registers the Phase 2C economic event schemas + payload mappers. Covers
/// Revenue, Distribution, Payout, and Vault/Account. Domain events live under
/// Whycespace.Domain.EconomicSystem.*; stored schemas live under
/// Whycespace.Shared.Contracts.Events.Economic.*.
/// </summary>
public sealed class EconomicSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        // ── Revenue ──────────────────────────────────────────────
        sink.RegisterSchema(
            "RevenueRecordedEvent",
            EventVersion.Default,
            typeof(RevenueDomain.RevenueRecordedEvent),
            typeof(RevenueRecordedEventSchema));

        sink.RegisterPayloadMapper("RevenueRecordedEvent", e =>
        {
            var evt = (RevenueDomain.RevenueRecordedEvent)e;
            return new RevenueRecordedEventSchema(
                Guid.Parse(evt.RevenueId),
                evt.SpvId,
                evt.Amount,
                evt.Currency,
                evt.SourceRef);
        });

        // ── Distribution ─────────────────────────────────────────
        sink.RegisterSchema(
            "DistributionCreatedEvent",
            EventVersion.Default,
            typeof(DistributionDomain.DistributionCreatedEvent),
            typeof(DistributionCreatedEventSchema));

        sink.RegisterPayloadMapper("DistributionCreatedEvent", e =>
        {
            var evt = (DistributionDomain.DistributionCreatedEvent)e;
            return new DistributionCreatedEventSchema(
                Guid.Parse(evt.DistributionId),
                evt.SpvId,
                evt.TotalAmount);
        });

        // ── Payout ───────────────────────────────────────────────
        sink.RegisterSchema(
            "PayoutExecutedEvent",
            EventVersion.Default,
            typeof(PayoutDomain.PayoutExecutedEvent),
            typeof(PayoutExecutedEventSchema));

        sink.RegisterPayloadMapper("PayoutExecutedEvent", e =>
        {
            var evt = (PayoutDomain.PayoutExecutedEvent)e;
            return new PayoutExecutedEventSchema(
                Guid.Parse(evt.PayoutId),
                Guid.Parse(evt.DistributionId));
        });

        // ── Vault / Account ──────────────────────────────────────
        sink.RegisterSchema(
            "VaultAccountCreatedEvent",
            EventVersion.Default,
            typeof(VaultDomain.VaultAccountCreatedEvent),
            typeof(VaultAccountCreatedEventSchema));

        sink.RegisterPayloadMapper("VaultAccountCreatedEvent", e =>
        {
            var evt = (VaultDomain.VaultAccountCreatedEvent)e;
            return new VaultAccountCreatedEventSchema(
                evt.VaultAccountId.Value,
                evt.OwnerSubjectId.Value,
                evt.Currency.Code);
        });

        sink.RegisterSchema(
            "VaultFundedEvent",
            EventVersion.Default,
            typeof(VaultDomain.VaultFundedEvent),
            typeof(VaultFundedEventSchema));

        sink.RegisterPayloadMapper("VaultFundedEvent", e =>
        {
            var evt = (VaultDomain.VaultFundedEvent)e;
            return new VaultFundedEventSchema(
                Guid.Parse(evt.VaultId),
                evt.Amount,
                evt.Currency);
        });

        sink.RegisterSchema(
            "CapitalAllocatedToSliceEvent",
            EventVersion.Default,
            typeof(VaultDomain.CapitalAllocatedToSliceEvent),
            typeof(CapitalAllocatedToSliceEventSchema));

        sink.RegisterPayloadMapper("CapitalAllocatedToSliceEvent", e =>
        {
            var evt = (VaultDomain.CapitalAllocatedToSliceEvent)e;
            return new CapitalAllocatedToSliceEventSchema(
                Guid.Parse(evt.VaultId),
                evt.Amount,
                MapSlice(evt.FromSlice),
                MapSlice(evt.ToSlice));
        });

        sink.RegisterSchema(
            "SpvProfitReceivedEvent",
            EventVersion.Default,
            typeof(VaultDomain.SpvProfitReceivedEvent),
            typeof(SpvProfitReceivedEventSchema));

        sink.RegisterPayloadMapper("SpvProfitReceivedEvent", e =>
        {
            var evt = (VaultDomain.SpvProfitReceivedEvent)e;
            return new SpvProfitReceivedEventSchema(
                Guid.Parse(evt.VaultId),
                evt.Amount,
                evt.Currency,
                MapSlice(evt.Slice));
        });

        sink.RegisterSchema(
            "VaultDebitedEvent",
            EventVersion.Default,
            typeof(VaultDomain.VaultDebitedEvent),
            typeof(VaultDebitedEventSchema));

        sink.RegisterPayloadMapper("VaultDebitedEvent", e =>
        {
            var evt = (VaultDomain.VaultDebitedEvent)e;
            return new VaultDebitedEventSchema(
                Guid.Parse(evt.VaultId),
                evt.Amount,
                MapSlice(evt.Slice));
        });

        sink.RegisterSchema(
            "VaultCreditedEvent",
            EventVersion.Default,
            typeof(VaultDomain.VaultCreditedEvent),
            typeof(VaultCreditedEventSchema));

        sink.RegisterPayloadMapper("VaultCreditedEvent", e =>
        {
            var evt = (VaultDomain.VaultCreditedEvent)e;
            return new VaultCreditedEventSchema(
                Guid.Parse(evt.VaultId),
                evt.Amount,
                MapSlice(evt.Slice));
        });

        // ── Ledger / Journal ─────────────────────────────────────
        sink.RegisterSchema(
            "JournalEntryAddedEvent",
            EventVersion.Default,
            typeof(JournalDomain.JournalEntryAddedEvent),
            typeof(JournalEntryRecordedEventSchema));

        sink.RegisterPayloadMapper("JournalEntryAddedEvent", e =>
        {
            var evt = (JournalDomain.JournalEntryAddedEvent)e;
            return new JournalEntryRecordedEventSchema(
                evt.JournalId.Value,
                evt.EntryId,
                evt.AccountId,
                evt.Amount.Value,
                evt.Currency.Code,
                evt.Direction.ToString());
        });

        sink.RegisterSchema(
            "JournalPostedEvent",
            EventVersion.Default,
            typeof(JournalDomain.JournalPostedEvent),
            typeof(JournalPostedEventSchema));

        sink.RegisterPayloadMapper("JournalPostedEvent", e =>
        {
            var evt = (JournalDomain.JournalPostedEvent)e;
            return new JournalPostedEventSchema(
                evt.JournalId.Value,
                evt.TotalDebit.Value,
                evt.TotalCredit.Value,
                string.Empty);
        });

        // ── Ledger / Ledger ──────────────────────────────────────
        sink.RegisterSchema(
            "LedgerOpenedEvent",
            EventVersion.Default,
            typeof(LedgerDomain.LedgerOpenedEvent),
            typeof(LedgerOpenedEventSchema));

        sink.RegisterPayloadMapper("LedgerOpenedEvent", e =>
        {
            var evt = (LedgerDomain.LedgerOpenedEvent)e;
            return new LedgerOpenedEventSchema(
                evt.LedgerId.Value,
                evt.Currency.Code);
        });

        sink.RegisterSchema(
            "LedgerUpdatedEvent",
            EventVersion.Default,
            typeof(LedgerDomain.LedgerUpdatedEvent),
            typeof(LedgerUpdatedEventSchema));

        sink.RegisterPayloadMapper("LedgerUpdatedEvent", e =>
        {
            var evt = (LedgerDomain.LedgerUpdatedEvent)e;
            return new LedgerUpdatedEventSchema(
                evt.LedgerId.Value,
                evt.JournalId,
                evt.JournalCount);
        });

        sink.RegisterSchema(
            "JournalAppendedToLedgerEvent",
            EventVersion.Default,
            typeof(LedgerDomain.JournalAppendedToLedgerEvent),
            typeof(JournalAppendedToLedgerEventSchema));

        sink.RegisterPayloadMapper("JournalAppendedToLedgerEvent", e =>
        {
            var evt = (LedgerDomain.JournalAppendedToLedgerEvent)e;
            return new JournalAppendedToLedgerEventSchema(
                evt.LedgerId.Value,
                evt.JournalId,
                evt.AppendedAt.Value);
        });

        RegisterLedgerEntry(sink);
        RegisterLedgerObligation(sink);
        RegisterLedgerTreasury(sink);

        RegisterCapitalAccount(sink);
        RegisterCapitalAllocation(sink);
        RegisterCapitalAsset(sink);
        RegisterCapitalBinding(sink);
        RegisterCapitalPool(sink);
        RegisterCapitalReserve(sink);
        RegisterCapitalVault(sink);
        RegisterComplianceAudit(sink);
        RegisterEnforcementRule(sink);
        RegisterEnforcementViolation(sink);
        RegisterEnforcementEscalation(sink);
        RegisterEnforcementSanction(sink);
        RegisterEnforcementRestriction(sink);
        RegisterEnforcementLock(sink);
        RegisterRiskExposure(sink);
        RegisterRoutingPath(sink);
        RegisterRoutingExecution(sink);
        RegisterSubject(sink);
        RegisterExchangeFx(sink);
        RegisterExchangeRate(sink);
        RegisterReconciliationProcess(sink);
        RegisterReconciliationDiscrepancy(sink);
    }

    private static void RegisterReconciliationProcess(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "ReconciliationTriggeredEvent",
            EventVersion.Default,
            typeof(ReconciliationProcessDomain.ReconciliationTriggeredEvent),
            typeof(ReconciliationProcessSchema.ReconciliationTriggeredEventSchema));
        sink.RegisterPayloadMapper("ReconciliationTriggeredEvent", e =>
        {
            var evt = (ReconciliationProcessDomain.ReconciliationTriggeredEvent)e;
            return new ReconciliationProcessSchema.ReconciliationTriggeredEventSchema(
                evt.ProcessId.Value,
                evt.LedgerReference.Value,
                evt.ObservedReference.Value,
                evt.TriggeredAt.Value);
        });

        sink.RegisterSchema(
            "ReconciliationMatchedEvent",
            EventVersion.Default,
            typeof(ReconciliationProcessDomain.ReconciliationMatchedEvent),
            typeof(ReconciliationProcessSchema.ReconciliationMatchedEventSchema));
        sink.RegisterPayloadMapper("ReconciliationMatchedEvent", e =>
        {
            var evt = (ReconciliationProcessDomain.ReconciliationMatchedEvent)e;
            return new ReconciliationProcessSchema.ReconciliationMatchedEventSchema(evt.ProcessId.Value);
        });

        sink.RegisterSchema(
            "ReconciliationMismatchedEvent",
            EventVersion.Default,
            typeof(ReconciliationProcessDomain.ReconciliationMismatchedEvent),
            typeof(ReconciliationProcessSchema.ReconciliationMismatchedEventSchema));
        sink.RegisterPayloadMapper("ReconciliationMismatchedEvent", e =>
        {
            var evt = (ReconciliationProcessDomain.ReconciliationMismatchedEvent)e;
            return new ReconciliationProcessSchema.ReconciliationMismatchedEventSchema(evt.ProcessId.Value);
        });

        sink.RegisterSchema(
            "ReconciliationResolvedEvent",
            EventVersion.Default,
            typeof(ReconciliationProcessDomain.ReconciliationResolvedEvent),
            typeof(ReconciliationProcessSchema.ReconciliationResolvedEventSchema));
        sink.RegisterPayloadMapper("ReconciliationResolvedEvent", e =>
        {
            var evt = (ReconciliationProcessDomain.ReconciliationResolvedEvent)e;
            return new ReconciliationProcessSchema.ReconciliationResolvedEventSchema(evt.ProcessId.Value);
        });
    }

    private static void RegisterReconciliationDiscrepancy(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "DiscrepancyDetectedEvent",
            EventVersion.Default,
            typeof(ReconciliationDiscrepancyDomain.DiscrepancyDetectedEvent),
            typeof(ReconciliationDiscrepancySchema.DiscrepancyDetectedEventSchema));
        sink.RegisterPayloadMapper("DiscrepancyDetectedEvent", e =>
        {
            var evt = (ReconciliationDiscrepancyDomain.DiscrepancyDetectedEvent)e;
            return new ReconciliationDiscrepancySchema.DiscrepancyDetectedEventSchema(
                evt.DiscrepancyId.Value,
                evt.ProcessReference.Value,
                evt.Source.ToString(),
                evt.ExpectedValue.Value,
                evt.ActualValue.Value,
                evt.Difference.Value,
                evt.DetectedAt.Value);
        });

        sink.RegisterSchema(
            "DiscrepancyInvestigatedEvent",
            EventVersion.Default,
            typeof(ReconciliationDiscrepancyDomain.DiscrepancyInvestigatedEvent),
            typeof(ReconciliationDiscrepancySchema.DiscrepancyInvestigatedEventSchema));
        sink.RegisterPayloadMapper("DiscrepancyInvestigatedEvent", e =>
        {
            var evt = (ReconciliationDiscrepancyDomain.DiscrepancyInvestigatedEvent)e;
            return new ReconciliationDiscrepancySchema.DiscrepancyInvestigatedEventSchema(evt.DiscrepancyId.Value);
        });

        sink.RegisterSchema(
            "DiscrepancyResolvedEvent",
            EventVersion.Default,
            typeof(ReconciliationDiscrepancyDomain.DiscrepancyResolvedEvent),
            typeof(ReconciliationDiscrepancySchema.DiscrepancyResolvedEventSchema));
        sink.RegisterPayloadMapper("DiscrepancyResolvedEvent", e =>
        {
            var evt = (ReconciliationDiscrepancyDomain.DiscrepancyResolvedEvent)e;
            return new ReconciliationDiscrepancySchema.DiscrepancyResolvedEventSchema(
                evt.DiscrepancyId.Value,
                evt.Resolution);
        });
    }

    private static void RegisterExchangeFx(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "FxPairRegisteredEvent",
            EventVersion.Default,
            typeof(ExchangeFxDomain.FxPairRegisteredEvent),
            typeof(ExchangeFxSchema.FxPairRegisteredEventSchema));
        sink.RegisterPayloadMapper("FxPairRegisteredEvent", e =>
        {
            var evt = (ExchangeFxDomain.FxPairRegisteredEvent)e;
            return new ExchangeFxSchema.FxPairRegisteredEventSchema(
                evt.FxId.Value,
                evt.CurrencyPair.BaseCurrency.Code,
                evt.CurrencyPair.QuoteCurrency.Code);
        });

        sink.RegisterSchema(
            "FxPairActivatedEvent",
            EventVersion.Default,
            typeof(ExchangeFxDomain.FxPairActivatedEvent),
            typeof(ExchangeFxSchema.FxPairActivatedEventSchema));
        sink.RegisterPayloadMapper("FxPairActivatedEvent", e =>
        {
            var evt = (ExchangeFxDomain.FxPairActivatedEvent)e;
            return new ExchangeFxSchema.FxPairActivatedEventSchema(
                evt.FxId.Value,
                evt.ActivatedAt.Value);
        });

        sink.RegisterSchema(
            "FxPairDeactivatedEvent",
            EventVersion.Default,
            typeof(ExchangeFxDomain.FxPairDeactivatedEvent),
            typeof(ExchangeFxSchema.FxPairDeactivatedEventSchema));
        sink.RegisterPayloadMapper("FxPairDeactivatedEvent", e =>
        {
            var evt = (ExchangeFxDomain.FxPairDeactivatedEvent)e;
            return new ExchangeFxSchema.FxPairDeactivatedEventSchema(
                evt.FxId.Value,
                evt.DeactivatedAt.Value);
        });
    }

    private static void RegisterExchangeRate(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "ExchangeRateDefinedEvent",
            EventVersion.Default,
            typeof(ExchangeRateDomain.ExchangeRateDefinedEvent),
            typeof(ExchangeRateSchema.ExchangeRateDefinedEventSchema));
        sink.RegisterPayloadMapper("ExchangeRateDefinedEvent", e =>
        {
            var evt = (ExchangeRateDomain.ExchangeRateDefinedEvent)e;
            return new ExchangeRateSchema.ExchangeRateDefinedEventSchema(
                evt.RateId.Value,
                evt.BaseCurrency.Code,
                evt.QuoteCurrency.Code,
                evt.RateValue,
                evt.EffectiveAt.Value,
                evt.Version);
        });

        sink.RegisterSchema(
            "ExchangeRateActivatedEvent",
            EventVersion.Default,
            typeof(ExchangeRateDomain.ExchangeRateActivatedEvent),
            typeof(ExchangeRateSchema.ExchangeRateActivatedEventSchema));
        sink.RegisterPayloadMapper("ExchangeRateActivatedEvent", e =>
        {
            var evt = (ExchangeRateDomain.ExchangeRateActivatedEvent)e;
            return new ExchangeRateSchema.ExchangeRateActivatedEventSchema(
                evt.RateId.Value,
                evt.ActivatedAt.Value);
        });

        sink.RegisterSchema(
            "ExchangeRateExpiredEvent",
            EventVersion.Default,
            typeof(ExchangeRateDomain.ExchangeRateExpiredEvent),
            typeof(ExchangeRateSchema.ExchangeRateExpiredEventSchema));
        sink.RegisterPayloadMapper("ExchangeRateExpiredEvent", e =>
        {
            var evt = (ExchangeRateDomain.ExchangeRateExpiredEvent)e;
            return new ExchangeRateSchema.ExchangeRateExpiredEventSchema(
                evt.RateId.Value,
                evt.ExpiredAt.Value);
        });
    }

    private static void RegisterSubject(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "EconomicSubjectRegisteredEvent",
            EventVersion.Default,
            typeof(SubjectDomain.EconomicSubjectRegisteredEvent),
            typeof(SubjectSchema.EconomicSubjectRegisteredEventSchema));
        sink.RegisterPayloadMapper("EconomicSubjectRegisteredEvent", e =>
        {
            var evt = (SubjectDomain.EconomicSubjectRegisteredEvent)e;
            return new SubjectSchema.EconomicSubjectRegisteredEventSchema(
                evt.SubjectId.Value,
                evt.SubjectType.ToString(),
                evt.StructuralRef.RefType.ToString(),
                evt.StructuralRef.RefId,
                evt.EconomicRef.RefType.ToString(),
                evt.EconomicRef.RefId);
        });
    }

    private static void RegisterRoutingPath(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "RoutingPathDefinedEvent",
            EventVersion.Default,
            typeof(RoutingPathDomain.RoutingPathDefinedEvent),
            typeof(RoutingPathSchema.RoutingPathDefinedEventSchema));
        sink.RegisterPayloadMapper("RoutingPathDefinedEvent", e =>
        {
            var evt = (RoutingPathDomain.RoutingPathDefinedEvent)e;
            return new RoutingPathSchema.RoutingPathDefinedEventSchema(
                evt.PathId.Value,
                evt.PathType.ToString(),
                evt.Conditions,
                evt.Priority);
        });

        sink.RegisterSchema(
            "RoutingPathActivatedEvent",
            EventVersion.Default,
            typeof(RoutingPathDomain.RoutingPathActivatedEvent),
            typeof(RoutingPathSchema.RoutingPathActivatedEventSchema));
        sink.RegisterPayloadMapper("RoutingPathActivatedEvent", e =>
        {
            var evt = (RoutingPathDomain.RoutingPathActivatedEvent)e;
            return new RoutingPathSchema.RoutingPathActivatedEventSchema(
                evt.PathId.Value,
                evt.ActivatedAt.Value);
        });

        sink.RegisterSchema(
            "RoutingPathDisabledEvent",
            EventVersion.Default,
            typeof(RoutingPathDomain.RoutingPathDisabledEvent),
            typeof(RoutingPathSchema.RoutingPathDisabledEventSchema));
        sink.RegisterPayloadMapper("RoutingPathDisabledEvent", e =>
        {
            var evt = (RoutingPathDomain.RoutingPathDisabledEvent)e;
            return new RoutingPathSchema.RoutingPathDisabledEventSchema(
                evt.PathId.Value,
                evt.DisabledAt.Value);
        });
    }

    private static void RegisterRoutingExecution(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "ExecutionStartedEvent",
            EventVersion.Default,
            typeof(RoutingExecutionDomain.ExecutionStartedEvent),
            typeof(RoutingExecutionSchema.ExecutionStartedEventSchema));
        sink.RegisterPayloadMapper("ExecutionStartedEvent", e =>
        {
            var evt = (RoutingExecutionDomain.ExecutionStartedEvent)e;
            return new RoutingExecutionSchema.ExecutionStartedEventSchema(
                evt.ExecutionId.Value,
                evt.PathId.Value,
                evt.StartedAt.Value);
        });

        sink.RegisterSchema(
            "ExecutionCompletedEvent",
            EventVersion.Default,
            typeof(RoutingExecutionDomain.ExecutionCompletedEvent),
            typeof(RoutingExecutionSchema.ExecutionCompletedEventSchema));
        sink.RegisterPayloadMapper("ExecutionCompletedEvent", e =>
        {
            var evt = (RoutingExecutionDomain.ExecutionCompletedEvent)e;
            return new RoutingExecutionSchema.ExecutionCompletedEventSchema(
                evt.ExecutionId.Value,
                evt.CompletedAt.Value);
        });

        sink.RegisterSchema(
            "ExecutionFailedEvent",
            EventVersion.Default,
            typeof(RoutingExecutionDomain.ExecutionFailedEvent),
            typeof(RoutingExecutionSchema.ExecutionFailedEventSchema));
        sink.RegisterPayloadMapper("ExecutionFailedEvent", e =>
        {
            var evt = (RoutingExecutionDomain.ExecutionFailedEvent)e;
            return new RoutingExecutionSchema.ExecutionFailedEventSchema(
                evt.ExecutionId.Value,
                evt.Reason,
                evt.FailedAt.Value);
        });

        sink.RegisterSchema(
            "ExecutionAbortedEvent",
            EventVersion.Default,
            typeof(RoutingExecutionDomain.ExecutionAbortedEvent),
            typeof(RoutingExecutionSchema.ExecutionAbortedEventSchema));
        sink.RegisterPayloadMapper("ExecutionAbortedEvent", e =>
        {
            var evt = (RoutingExecutionDomain.ExecutionAbortedEvent)e;
            return new RoutingExecutionSchema.ExecutionAbortedEventSchema(
                evt.ExecutionId.Value,
                evt.Reason,
                evt.AbortedAt.Value);
        });
    }

    private static void RegisterRiskExposure(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "RiskExposureCreatedEvent",
            EventVersion.Default,
            typeof(RiskExposureDomain.ExposureCreatedEvent),
            typeof(RiskExposureSchema.RiskExposureCreatedEventSchema));
        sink.RegisterPayloadMapper("RiskExposureCreatedEvent", e =>
        {
            var evt = (RiskExposureDomain.ExposureCreatedEvent)e;
            return new RiskExposureSchema.RiskExposureCreatedEventSchema(
                evt.ExposureId.Value,
                evt.SourceId.Value,
                (int)evt.ExposureType,
                evt.TotalExposure.Value,
                evt.Currency.Code,
                evt.CreatedAt.Value);
        });

        sink.RegisterSchema(
            "RiskExposureIncreasedEvent",
            EventVersion.Default,
            typeof(RiskExposureDomain.ExposureIncreasedEvent),
            typeof(RiskExposureSchema.RiskExposureIncreasedEventSchema));
        sink.RegisterPayloadMapper("RiskExposureIncreasedEvent", e =>
        {
            var evt = (RiskExposureDomain.ExposureIncreasedEvent)e;
            return new RiskExposureSchema.RiskExposureIncreasedEventSchema(
                evt.ExposureId.Value,
                evt.IncreasedBy.Value,
                evt.NewTotal.Value);
        });

        sink.RegisterSchema(
            "RiskExposureReducedEvent",
            EventVersion.Default,
            typeof(RiskExposureDomain.ExposureReducedEvent),
            typeof(RiskExposureSchema.RiskExposureReducedEventSchema));
        sink.RegisterPayloadMapper("RiskExposureReducedEvent", e =>
        {
            var evt = (RiskExposureDomain.ExposureReducedEvent)e;
            return new RiskExposureSchema.RiskExposureReducedEventSchema(
                evt.ExposureId.Value,
                evt.ReducedBy.Value,
                evt.NewTotal.Value);
        });

        sink.RegisterSchema(
            "RiskExposureClosedEvent",
            EventVersion.Default,
            typeof(RiskExposureDomain.ExposureClosedEvent),
            typeof(RiskExposureSchema.RiskExposureClosedEventSchema));
        sink.RegisterPayloadMapper("RiskExposureClosedEvent", e =>
        {
            var evt = (RiskExposureDomain.ExposureClosedEvent)e;
            return new RiskExposureSchema.RiskExposureClosedEventSchema(evt.ExposureId.Value);
        });
    }

    private static void RegisterEnforcementRule(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "EnforcementRuleDefinedEvent",
            EventVersion.Default,
            typeof(EnforcementRuleDomain.EnforcementRuleDefinedEvent),
            typeof(EnforcementRuleSchema.EnforcementRuleDefinedEventSchema));
        sink.RegisterPayloadMapper("EnforcementRuleDefinedEvent", e =>
        {
            var evt = (EnforcementRuleDomain.EnforcementRuleDefinedEvent)e;
            return new EnforcementRuleSchema.EnforcementRuleDefinedEventSchema(
                evt.RuleId.Value,
                evt.RuleCode.Value,
                evt.RuleName,
                evt.RuleCategory.Value,
                evt.Scope.ToString(),
                evt.Severity.ToString(),
                evt.Description,
                evt.CreatedAt.Value);
        });

        sink.RegisterSchema(
            "EnforcementRuleActivatedEvent",
            EventVersion.Default,
            typeof(EnforcementRuleDomain.EnforcementRuleActivatedEvent),
            typeof(EnforcementRuleSchema.EnforcementRuleActivatedEventSchema));
        sink.RegisterPayloadMapper("EnforcementRuleActivatedEvent", e =>
        {
            var evt = (EnforcementRuleDomain.EnforcementRuleActivatedEvent)e;
            return new EnforcementRuleSchema.EnforcementRuleActivatedEventSchema(evt.RuleId.Value);
        });

        sink.RegisterSchema(
            "EnforcementRuleDisabledEvent",
            EventVersion.Default,
            typeof(EnforcementRuleDomain.EnforcementRuleDisabledEvent),
            typeof(EnforcementRuleSchema.EnforcementRuleDisabledEventSchema));
        sink.RegisterPayloadMapper("EnforcementRuleDisabledEvent", e =>
        {
            var evt = (EnforcementRuleDomain.EnforcementRuleDisabledEvent)e;
            return new EnforcementRuleSchema.EnforcementRuleDisabledEventSchema(evt.RuleId.Value);
        });

        sink.RegisterSchema(
            "EnforcementRuleRetiredEvent",
            EventVersion.Default,
            typeof(EnforcementRuleDomain.EnforcementRuleRetiredEvent),
            typeof(EnforcementRuleSchema.EnforcementRuleRetiredEventSchema));
        sink.RegisterPayloadMapper("EnforcementRuleRetiredEvent", e =>
        {
            var evt = (EnforcementRuleDomain.EnforcementRuleRetiredEvent)e;
            return new EnforcementRuleSchema.EnforcementRuleRetiredEventSchema(evt.RuleId.Value);
        });
    }

    private static void RegisterEnforcementViolation(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "ViolationDetectedEvent",
            EventVersion.Default,
            typeof(EnforcementViolationDomain.ViolationDetectedEvent),
            typeof(EnforcementViolationSchema.ViolationDetectedEventSchema));
        sink.RegisterPayloadMapper("ViolationDetectedEvent", e =>
        {
            var evt = (EnforcementViolationDomain.ViolationDetectedEvent)e;
            return new EnforcementViolationSchema.ViolationDetectedEventSchema(
                evt.ViolationId.Value,
                evt.RuleId.Value,
                evt.Source.Value,
                evt.Reason,
                evt.Severity.ToString(),
                evt.RecommendedAction.ToString(),
                evt.DetectedAt.Value);
        });

        sink.RegisterSchema(
            "ViolationAcknowledgedEvent",
            EventVersion.Default,
            typeof(EnforcementViolationDomain.ViolationAcknowledgedEvent),
            typeof(EnforcementViolationSchema.ViolationAcknowledgedEventSchema));
        sink.RegisterPayloadMapper("ViolationAcknowledgedEvent", e =>
        {
            var evt = (EnforcementViolationDomain.ViolationAcknowledgedEvent)e;
            return new EnforcementViolationSchema.ViolationAcknowledgedEventSchema(
                evt.ViolationId.Value,
                evt.AcknowledgedAt.Value);
        });

        sink.RegisterSchema(
            "ViolationResolvedEvent",
            EventVersion.Default,
            typeof(EnforcementViolationDomain.ViolationResolvedEvent),
            typeof(EnforcementViolationSchema.ViolationResolvedEventSchema));
        sink.RegisterPayloadMapper("ViolationResolvedEvent", e =>
        {
            var evt = (EnforcementViolationDomain.ViolationResolvedEvent)e;
            return new EnforcementViolationSchema.ViolationResolvedEventSchema(
                evt.ViolationId.Value,
                evt.Resolution,
                evt.ResolvedAt.Value);
        });
    }

    private static void RegisterEnforcementEscalation(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "EscalationInitializedEvent",
            EventVersion.Default,
            typeof(EnforcementEscalationDomain.EscalationInitializedEvent),
            typeof(EnforcementEscalationSchema.EscalationInitializedEventSchema));
        sink.RegisterPayloadMapper("EscalationInitializedEvent", e =>
        {
            var evt = (EnforcementEscalationDomain.EscalationInitializedEvent)e;
            return new EnforcementEscalationSchema.EscalationInitializedEventSchema(
                evt.SubjectId.Value,
                evt.Window.Start.Value,
                evt.Window.Duration.Ticks,
                evt.InitializedAt.Value);
        });

        sink.RegisterSchema(
            "ViolationAccumulatedEvent",
            EventVersion.Default,
            typeof(EnforcementEscalationDomain.ViolationAccumulatedEvent),
            typeof(EnforcementEscalationSchema.ViolationAccumulatedEventSchema));
        sink.RegisterPayloadMapper("ViolationAccumulatedEvent", e =>
        {
            var evt = (EnforcementEscalationDomain.ViolationAccumulatedEvent)e;
            return new EnforcementEscalationSchema.ViolationAccumulatedEventSchema(
                evt.SubjectId.Value,
                evt.ViolationId,
                evt.SeverityWeight,
                evt.NewCounter.Count,
                evt.NewCounter.SeverityScore,
                evt.AccumulatedAt.Value);
        });

        sink.RegisterSchema(
            "EscalationLevelIncreasedEvent",
            EventVersion.Default,
            typeof(EnforcementEscalationDomain.EscalationLevelIncreasedEvent),
            typeof(EnforcementEscalationSchema.EscalationLevelIncreasedEventSchema));
        sink.RegisterPayloadMapper("EscalationLevelIncreasedEvent", e =>
        {
            var evt = (EnforcementEscalationDomain.EscalationLevelIncreasedEvent)e;
            return new EnforcementEscalationSchema.EscalationLevelIncreasedEventSchema(
                evt.SubjectId.Value,
                evt.PreviousLevel.ToString(),
                evt.NewLevel.ToString(),
                evt.Counter.Count,
                evt.Counter.SeverityScore,
                evt.EscalatedAt.Value);
        });

        sink.RegisterSchema(
            "EscalationResetEvent",
            EventVersion.Default,
            typeof(EnforcementEscalationDomain.EscalationResetEvent),
            typeof(EnforcementEscalationSchema.EscalationResetEventSchema));
        sink.RegisterPayloadMapper("EscalationResetEvent", e =>
        {
            var evt = (EnforcementEscalationDomain.EscalationResetEvent)e;
            return new EnforcementEscalationSchema.EscalationResetEventSchema(
                evt.SubjectId.Value,
                evt.NewWindow.Start.Value,
                evt.NewWindow.Duration.Ticks,
                evt.ResetAt.Value);
        });
    }

    private static void RegisterEnforcementSanction(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "SanctionIssuedEvent",
            EventVersion.Default,
            typeof(EnforcementSanctionDomain.SanctionIssuedEvent),
            typeof(EnforcementSanctionSchema.SanctionIssuedEventSchema));
        sink.RegisterPayloadMapper("SanctionIssuedEvent", e =>
        {
            var evt = (EnforcementSanctionDomain.SanctionIssuedEvent)e;
            return new EnforcementSanctionSchema.SanctionIssuedEventSchema(
                evt.SanctionId.Value,
                evt.SubjectId.Value,
                evt.Type.ToString(),
                evt.Scope.ToString(),
                evt.Reason.Value,
                evt.Period.EffectiveAt.Value,
                evt.Period.ExpiresAt?.Value,
                evt.IssuedAt.Value);
        });

        sink.RegisterSchema(
            "SanctionActivatedEvent",
            EventVersion.Default,
            typeof(EnforcementSanctionDomain.SanctionActivatedEvent),
            typeof(EnforcementSanctionSchema.SanctionActivatedEventSchema));
        sink.RegisterPayloadMapper("SanctionActivatedEvent", e =>
        {
            var evt = (EnforcementSanctionDomain.SanctionActivatedEvent)e;
            return new EnforcementSanctionSchema.SanctionActivatedEventSchema(
                evt.SanctionId.Value,
                evt.SubjectId.Value,
                evt.ActivatedAt.Value);
        });

        sink.RegisterSchema(
            "SanctionExpiredEvent",
            EventVersion.Default,
            typeof(EnforcementSanctionDomain.SanctionExpiredEvent),
            typeof(EnforcementSanctionSchema.SanctionExpiredEventSchema));
        sink.RegisterPayloadMapper("SanctionExpiredEvent", e =>
        {
            var evt = (EnforcementSanctionDomain.SanctionExpiredEvent)e;
            return new EnforcementSanctionSchema.SanctionExpiredEventSchema(
                evt.SanctionId.Value,
                evt.SubjectId.Value,
                evt.ExpiredAt.Value);
        });

        sink.RegisterSchema(
            "SanctionRevokedEvent",
            EventVersion.Default,
            typeof(EnforcementSanctionDomain.SanctionRevokedEvent),
            typeof(EnforcementSanctionSchema.SanctionRevokedEventSchema));
        sink.RegisterPayloadMapper("SanctionRevokedEvent", e =>
        {
            var evt = (EnforcementSanctionDomain.SanctionRevokedEvent)e;
            return new EnforcementSanctionSchema.SanctionRevokedEventSchema(
                evt.SanctionId.Value,
                evt.SubjectId.Value,
                evt.RevocationReason.Value,
                evt.RevokedAt.Value);
        });
    }

    private static void RegisterEnforcementRestriction(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "RestrictionAppliedEvent",
            EventVersion.Default,
            typeof(EnforcementRestrictionDomain.RestrictionAppliedEvent),
            typeof(EnforcementRestrictionSchema.RestrictionAppliedEventSchema));
        sink.RegisterPayloadMapper("RestrictionAppliedEvent", e =>
        {
            var evt = (EnforcementRestrictionDomain.RestrictionAppliedEvent)e;
            return new EnforcementRestrictionSchema.RestrictionAppliedEventSchema(
                evt.RestrictionId.Value,
                evt.SubjectId.Value,
                evt.Scope.ToString(),
                evt.Reason.Value,
                evt.AppliedAt.Value);
        });

        sink.RegisterSchema(
            "RestrictionUpdatedEvent",
            EventVersion.Default,
            typeof(EnforcementRestrictionDomain.RestrictionUpdatedEvent),
            typeof(EnforcementRestrictionSchema.RestrictionUpdatedEventSchema));
        sink.RegisterPayloadMapper("RestrictionUpdatedEvent", e =>
        {
            var evt = (EnforcementRestrictionDomain.RestrictionUpdatedEvent)e;
            return new EnforcementRestrictionSchema.RestrictionUpdatedEventSchema(
                evt.RestrictionId.Value,
                evt.SubjectId.Value,
                evt.NewScope.ToString(),
                evt.NewReason.Value,
                evt.UpdatedAt.Value);
        });

        sink.RegisterSchema(
            "RestrictionRemovedEvent",
            EventVersion.Default,
            typeof(EnforcementRestrictionDomain.RestrictionRemovedEvent),
            typeof(EnforcementRestrictionSchema.RestrictionRemovedEventSchema));
        sink.RegisterPayloadMapper("RestrictionRemovedEvent", e =>
        {
            var evt = (EnforcementRestrictionDomain.RestrictionRemovedEvent)e;
            return new EnforcementRestrictionSchema.RestrictionRemovedEventSchema(
                evt.RestrictionId.Value,
                evt.SubjectId.Value,
                evt.RemovalReason.Value,
                evt.RemovedAt.Value);
        });
    }

    private static void RegisterEnforcementLock(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "SystemLockedEvent",
            EventVersion.Default,
            typeof(EnforcementLockDomain.SystemLockedEvent),
            typeof(EnforcementLockSchema.SystemLockedEventSchema));
        sink.RegisterPayloadMapper("SystemLockedEvent", e =>
        {
            var evt = (EnforcementLockDomain.SystemLockedEvent)e;
            return new EnforcementLockSchema.SystemLockedEventSchema(
                evt.LockId.Value,
                evt.SubjectId.Value,
                evt.Scope.ToString(),
                evt.Reason.Value,
                evt.LockedAt.Value);
        });

        sink.RegisterSchema(
            "SystemUnlockedEvent",
            EventVersion.Default,
            typeof(EnforcementLockDomain.SystemUnlockedEvent),
            typeof(EnforcementLockSchema.SystemUnlockedEventSchema));
        sink.RegisterPayloadMapper("SystemUnlockedEvent", e =>
        {
            var evt = (EnforcementLockDomain.SystemUnlockedEvent)e;
            return new EnforcementLockSchema.SystemUnlockedEventSchema(
                evt.LockId.Value,
                evt.SubjectId.Value,
                evt.UnlockReason.Value,
                evt.UnlockedAt.Value);
        });
    }

    private static void RegisterComplianceAudit(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "AuditRecordCreatedEvent",
            EventVersion.Default,
            typeof(AuditDomain.AuditRecordCreatedEvent),
            typeof(AuditSchema.AuditRecordCreatedEventSchema));
        sink.RegisterPayloadMapper("AuditRecordCreatedEvent", e =>
        {
            var evt = (AuditDomain.AuditRecordCreatedEvent)e;
            return new AuditSchema.AuditRecordCreatedEventSchema(
                evt.AuditRecordId.Value,
                evt.SourceDomain.Value,
                evt.SourceAggregateId.Value,
                evt.SourceEventId.Value,
                evt.AuditType.ToString(),
                evt.EvidenceSummary.Value,
                evt.RecordedAt.Value);
        });

        sink.RegisterSchema(
            "AuditRecordFinalizedEvent",
            EventVersion.Default,
            typeof(AuditDomain.AuditRecordFinalizedEvent),
            typeof(AuditSchema.AuditRecordFinalizedEventSchema));
        sink.RegisterPayloadMapper("AuditRecordFinalizedEvent", e =>
        {
            var evt = (AuditDomain.AuditRecordFinalizedEvent)e;
            return new AuditSchema.AuditRecordFinalizedEventSchema(
                evt.AuditRecordId.Value,
                evt.FinalizedAt.Value);
        });
    }

    private static void RegisterCapitalAccount(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "CapitalAccountOpenedEvent",
            EventVersion.Default,
            typeof(CapitalAccountDomain.CapitalAccountOpenedEvent),
            typeof(CapitalAccountSchema.CapitalAccountOpenedEventSchema));
        sink.RegisterPayloadMapper("CapitalAccountOpenedEvent", e =>
        {
            var evt = (CapitalAccountDomain.CapitalAccountOpenedEvent)e;
            return new CapitalAccountSchema.CapitalAccountOpenedEventSchema(
                evt.AccountId.Value, evt.OwnerId.Value, evt.Currency.Code, evt.CreatedAt.Value);
        });

        sink.RegisterSchema(
            "CapitalFundedEvent",
            EventVersion.Default,
            typeof(CapitalAccountDomain.CapitalFundedEvent),
            typeof(CapitalAccountSchema.CapitalFundedEventSchema));
        sink.RegisterPayloadMapper("CapitalFundedEvent", e =>
        {
            var evt = (CapitalAccountDomain.CapitalFundedEvent)e;
            return new CapitalAccountSchema.CapitalFundedEventSchema(
                evt.AccountId.Value, evt.FundedAmount.Value, evt.NewTotalBalance.Value, evt.NewAvailableBalance.Value);
        });

        sink.RegisterSchema(
            "AccountCapitalAllocatedEvent",
            EventVersion.Default,
            typeof(CapitalAccountDomain.AccountCapitalAllocatedEvent),
            typeof(CapitalAccountSchema.AccountCapitalAllocatedEventSchema));
        sink.RegisterPayloadMapper("AccountCapitalAllocatedEvent", e =>
        {
            var evt = (CapitalAccountDomain.AccountCapitalAllocatedEvent)e;
            return new CapitalAccountSchema.AccountCapitalAllocatedEventSchema(
                evt.AccountId.Value, evt.AllocatedAmount.Value, evt.NewAvailableBalance.Value);
        });

        sink.RegisterSchema(
            "AccountCapitalReservedEvent",
            EventVersion.Default,
            typeof(CapitalAccountDomain.AccountCapitalReservedEvent),
            typeof(CapitalAccountSchema.AccountCapitalReservedEventSchema));
        sink.RegisterPayloadMapper("AccountCapitalReservedEvent", e =>
        {
            var evt = (CapitalAccountDomain.AccountCapitalReservedEvent)e;
            return new CapitalAccountSchema.AccountCapitalReservedEventSchema(
                evt.AccountId.Value, evt.ReservedAmount.Value, evt.NewAvailableBalance.Value, evt.NewReservedBalance.Value);
        });

        sink.RegisterSchema(
            "AccountReservationReleasedEvent",
            EventVersion.Default,
            typeof(CapitalAccountDomain.AccountReservationReleasedEvent),
            typeof(CapitalAccountSchema.AccountReservationReleasedEventSchema));
        sink.RegisterPayloadMapper("AccountReservationReleasedEvent", e =>
        {
            var evt = (CapitalAccountDomain.AccountReservationReleasedEvent)e;
            return new CapitalAccountSchema.AccountReservationReleasedEventSchema(
                evt.AccountId.Value, evt.ReleasedAmount.Value, evt.NewAvailableBalance.Value, evt.NewReservedBalance.Value);
        });

        sink.RegisterSchema(
            "CapitalAccountFrozenEvent",
            EventVersion.Default,
            typeof(CapitalAccountDomain.CapitalAccountFrozenEvent),
            typeof(CapitalAccountSchema.CapitalAccountFrozenEventSchema));
        sink.RegisterPayloadMapper("CapitalAccountFrozenEvent", e =>
        {
            var evt = (CapitalAccountDomain.CapitalAccountFrozenEvent)e;
            return new CapitalAccountSchema.CapitalAccountFrozenEventSchema(evt.AccountId.Value, evt.Reason);
        });

        sink.RegisterSchema(
            "CapitalAccountClosedEvent",
            EventVersion.Default,
            typeof(CapitalAccountDomain.CapitalAccountClosedEvent),
            typeof(CapitalAccountSchema.CapitalAccountClosedEventSchema));
        sink.RegisterPayloadMapper("CapitalAccountClosedEvent", e =>
        {
            var evt = (CapitalAccountDomain.CapitalAccountClosedEvent)e;
            return new CapitalAccountSchema.CapitalAccountClosedEventSchema(evt.AccountId.Value, evt.ClosedAt.Value);
        });
    }

    private static void RegisterCapitalAllocation(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "AllocationCreatedEvent",
            EventVersion.Default,
            typeof(CapitalAllocationDomain.AllocationCreatedEvent),
            typeof(CapitalAllocationSchema.AllocationCreatedEventSchema));
        sink.RegisterPayloadMapper("AllocationCreatedEvent", e =>
        {
            var evt = (CapitalAllocationDomain.AllocationCreatedEvent)e;
            return new CapitalAllocationSchema.AllocationCreatedEventSchema(
                evt.AllocationId.Value, evt.SourceAccountId, evt.TargetId.Value,
                evt.Amount.Value, evt.Currency.Code, evt.AllocatedAt.Value);
        });

        sink.RegisterSchema(
            "AllocationReleasedEvent",
            EventVersion.Default,
            typeof(CapitalAllocationDomain.AllocationReleasedEvent),
            typeof(CapitalAllocationSchema.AllocationReleasedEventSchema));
        sink.RegisterPayloadMapper("AllocationReleasedEvent", e =>
        {
            var evt = (CapitalAllocationDomain.AllocationReleasedEvent)e;
            return new CapitalAllocationSchema.AllocationReleasedEventSchema(
                evt.AllocationId.Value, evt.SourceAccountId, evt.ReleasedAmount.Value, evt.ReleasedAt.Value);
        });

        sink.RegisterSchema(
            "AllocationCompletedEvent",
            EventVersion.Default,
            typeof(CapitalAllocationDomain.AllocationCompletedEvent),
            typeof(CapitalAllocationSchema.AllocationCompletedEventSchema));
        sink.RegisterPayloadMapper("AllocationCompletedEvent", e =>
        {
            var evt = (CapitalAllocationDomain.AllocationCompletedEvent)e;
            return new CapitalAllocationSchema.AllocationCompletedEventSchema(
                evt.AllocationId.Value, evt.CompletedAt.Value);
        });

        sink.RegisterSchema(
            "CapitalAllocatedToSpvEvent",
            EventVersion.Default,
            typeof(CapitalAllocationDomain.CapitalAllocatedToSpvEvent),
            typeof(CapitalAllocationSchema.CapitalAllocatedToSpvEventSchema));
        sink.RegisterPayloadMapper("CapitalAllocatedToSpvEvent", e =>
        {
            var evt = (CapitalAllocationDomain.CapitalAllocatedToSpvEvent)e;
            return new CapitalAllocationSchema.CapitalAllocatedToSpvEventSchema(
                Guid.Parse(evt.AllocationId), evt.TargetId, evt.OwnershipPercentage);
        });
    }

    private static void RegisterCapitalAsset(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "AssetCreatedEvent",
            EventVersion.Default,
            typeof(CapitalAssetDomain.AssetCreatedEvent),
            typeof(CapitalAssetSchema.AssetCreatedEventSchema));
        sink.RegisterPayloadMapper("AssetCreatedEvent", e =>
        {
            var evt = (CapitalAssetDomain.AssetCreatedEvent)e;
            return new CapitalAssetSchema.AssetCreatedEventSchema(
                evt.AssetId.Value, evt.OwnerId, evt.InitialValue.Value, evt.Currency.Code, evt.CreatedAt.Value);
        });

        sink.RegisterSchema(
            "AssetValuedEvent",
            EventVersion.Default,
            typeof(CapitalAssetDomain.AssetValuedEvent),
            typeof(CapitalAssetSchema.AssetValuedEventSchema));
        sink.RegisterPayloadMapper("AssetValuedEvent", e =>
        {
            var evt = (CapitalAssetDomain.AssetValuedEvent)e;
            return new CapitalAssetSchema.AssetValuedEventSchema(
                evt.AssetId.Value, evt.PreviousValue.Value, evt.NewValue.Value, evt.Currency.Code, evt.ValuedAt.Value);
        });

        sink.RegisterSchema(
            "AssetDisposedEvent",
            EventVersion.Default,
            typeof(CapitalAssetDomain.AssetDisposedEvent),
            typeof(CapitalAssetSchema.AssetDisposedEventSchema));
        sink.RegisterPayloadMapper("AssetDisposedEvent", e =>
        {
            var evt = (CapitalAssetDomain.AssetDisposedEvent)e;
            return new CapitalAssetSchema.AssetDisposedEventSchema(
                evt.AssetId.Value, evt.FinalValue.Value, evt.DisposedAt.Value);
        });
    }

    private static void RegisterCapitalBinding(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "CapitalBoundEvent",
            EventVersion.Default,
            typeof(CapitalBindingDomain.CapitalBoundEvent),
            typeof(CapitalBindingSchema.CapitalBoundEventSchema));
        sink.RegisterPayloadMapper("CapitalBoundEvent", e =>
        {
            var evt = (CapitalBindingDomain.CapitalBoundEvent)e;
            return new CapitalBindingSchema.CapitalBoundEventSchema(
                evt.BindingId.Value, evt.AccountId, evt.OwnerId, (int)evt.OwnershipType, evt.BoundAt.Value);
        });

        sink.RegisterSchema(
            "OwnershipTransferredEvent",
            EventVersion.Default,
            typeof(CapitalBindingDomain.OwnershipTransferredEvent),
            typeof(CapitalBindingSchema.OwnershipTransferredEventSchema));
        sink.RegisterPayloadMapper("OwnershipTransferredEvent", e =>
        {
            var evt = (CapitalBindingDomain.OwnershipTransferredEvent)e;
            return new CapitalBindingSchema.OwnershipTransferredEventSchema(
                evt.BindingId.Value, evt.PreviousOwnerId, evt.NewOwnerId, (int)evt.NewOwnershipType, evt.TransferredAt.Value);
        });

        sink.RegisterSchema(
            "BindingReleasedEvent",
            EventVersion.Default,
            typeof(CapitalBindingDomain.BindingReleasedEvent),
            typeof(CapitalBindingSchema.BindingReleasedEventSchema));
        sink.RegisterPayloadMapper("BindingReleasedEvent", e =>
        {
            var evt = (CapitalBindingDomain.BindingReleasedEvent)e;
            return new CapitalBindingSchema.BindingReleasedEventSchema(
                evt.BindingId.Value, evt.AccountId, evt.ReleasedAt.Value);
        });
    }

    private static void RegisterCapitalPool(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "PoolCreatedEvent",
            EventVersion.Default,
            typeof(CapitalPoolDomain.PoolCreatedEvent),
            typeof(CapitalPoolSchema.PoolCreatedEventSchema));
        sink.RegisterPayloadMapper("PoolCreatedEvent", e =>
        {
            var evt = (CapitalPoolDomain.PoolCreatedEvent)e;
            return new CapitalPoolSchema.PoolCreatedEventSchema(
                evt.PoolId.Value, evt.Currency.Code, evt.CreatedAt.Value);
        });

        sink.RegisterSchema(
            "CapitalAggregatedEvent",
            EventVersion.Default,
            typeof(CapitalPoolDomain.CapitalAggregatedEvent),
            typeof(CapitalPoolSchema.CapitalAggregatedEventSchema));
        sink.RegisterPayloadMapper("CapitalAggregatedEvent", e =>
        {
            var evt = (CapitalPoolDomain.CapitalAggregatedEvent)e;
            return new CapitalPoolSchema.CapitalAggregatedEventSchema(
                evt.PoolId.Value, evt.SourceAccountId, evt.AggregatedAmount.Value, evt.NewPoolTotal.Value);
        });

        sink.RegisterSchema(
            "CapitalReducedEvent",
            EventVersion.Default,
            typeof(CapitalPoolDomain.CapitalReducedEvent),
            typeof(CapitalPoolSchema.CapitalReducedEventSchema));
        sink.RegisterPayloadMapper("CapitalReducedEvent", e =>
        {
            var evt = (CapitalPoolDomain.CapitalReducedEvent)e;
            return new CapitalPoolSchema.CapitalReducedEventSchema(
                evt.PoolId.Value, evt.SourceAccountId, evt.ReducedAmount.Value, evt.NewPoolTotal.Value);
        });
    }

    private static void RegisterCapitalReserve(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "ReserveCreatedEvent",
            EventVersion.Default,
            typeof(CapitalReserveDomain.ReserveCreatedEvent),
            typeof(CapitalReserveSchema.ReserveCreatedEventSchema));
        sink.RegisterPayloadMapper("ReserveCreatedEvent", e =>
        {
            var evt = (CapitalReserveDomain.ReserveCreatedEvent)e;
            return new CapitalReserveSchema.ReserveCreatedEventSchema(
                evt.ReserveId.Value, evt.AccountId, evt.ReservedAmount.Value,
                evt.Currency.Code, evt.ReservedAt.Value, evt.ExpiresAt.Value);
        });

        sink.RegisterSchema(
            "ReserveReleasedEvent",
            EventVersion.Default,
            typeof(CapitalReserveDomain.ReserveReleasedEvent),
            typeof(CapitalReserveSchema.ReserveReleasedEventSchema));
        sink.RegisterPayloadMapper("ReserveReleasedEvent", e =>
        {
            var evt = (CapitalReserveDomain.ReserveReleasedEvent)e;
            return new CapitalReserveSchema.ReserveReleasedEventSchema(
                evt.ReserveId.Value, evt.AccountId, evt.ReleasedAmount.Value, evt.ReleasedAt.Value);
        });

        sink.RegisterSchema(
            "ReserveExpiredEvent",
            EventVersion.Default,
            typeof(CapitalReserveDomain.ReserveExpiredEvent),
            typeof(CapitalReserveSchema.ReserveExpiredEventSchema));
        sink.RegisterPayloadMapper("ReserveExpiredEvent", e =>
        {
            var evt = (CapitalReserveDomain.ReserveExpiredEvent)e;
            return new CapitalReserveSchema.ReserveExpiredEventSchema(
                evt.ReserveId.Value, evt.AccountId, evt.ExpiredAmount.Value, evt.ExpiredAt.Value);
        });
    }

    private static void RegisterCapitalVault(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "VaultCreatedEvent",
            EventVersion.Default,
            typeof(CapitalVaultDomain.VaultCreatedEvent),
            typeof(CapitalVaultSchema.VaultCreatedEventSchema));
        sink.RegisterPayloadMapper("VaultCreatedEvent", e =>
        {
            var evt = (CapitalVaultDomain.VaultCreatedEvent)e;
            return new CapitalVaultSchema.VaultCreatedEventSchema(
                evt.VaultId.Value, evt.OwnerId, evt.Currency.Code, evt.CreatedAt.Value);
        });

        sink.RegisterSchema(
            "VaultSliceCreatedEvent",
            EventVersion.Default,
            typeof(CapitalVaultDomain.VaultSliceCreatedEvent),
            typeof(CapitalVaultSchema.VaultSliceCreatedEventSchema));
        sink.RegisterPayloadMapper("VaultSliceCreatedEvent", e =>
        {
            var evt = (CapitalVaultDomain.VaultSliceCreatedEvent)e;
            return new CapitalVaultSchema.VaultSliceCreatedEventSchema(
                evt.VaultId.Value, evt.SliceId.Value, evt.TotalCapacity.Value, evt.Currency.Code);
        });

        sink.RegisterSchema(
            "CapitalDepositedEvent",
            EventVersion.Default,
            typeof(CapitalVaultDomain.CapitalDepositedEvent),
            typeof(CapitalVaultSchema.CapitalDepositedEventSchema));
        sink.RegisterPayloadMapper("CapitalDepositedEvent", e =>
        {
            var evt = (CapitalVaultDomain.CapitalDepositedEvent)e;
            return new CapitalVaultSchema.CapitalDepositedEventSchema(
                evt.VaultId.Value, evt.SliceId.Value, evt.DepositedAmount.Value,
                evt.NewSliceCapacity.Value, evt.NewVaultTotal.Value);
        });

        sink.RegisterSchema(
            "CapitalAllocatedToSliceEvent",
            EventVersion.Default,
            typeof(CapitalVaultDomain.CapitalAllocatedToSliceEvent),
            typeof(CapitalVaultSchema.CapitalAllocatedToSliceEventSchema));
        sink.RegisterPayloadMapper("CapitalAllocatedToSliceEvent", e =>
        {
            var evt = (CapitalVaultDomain.CapitalAllocatedToSliceEvent)e;
            return new CapitalVaultSchema.CapitalAllocatedToSliceEventSchema(
                evt.VaultId.Value, evt.SliceId.Value, evt.AllocatedAmount.Value,
                evt.NewSliceAvailable.Value, evt.NewSliceUsed.Value);
        });

        sink.RegisterSchema(
            "CapitalReleasedFromSliceEvent",
            EventVersion.Default,
            typeof(CapitalVaultDomain.CapitalReleasedFromSliceEvent),
            typeof(CapitalVaultSchema.CapitalReleasedFromSliceEventSchema));
        sink.RegisterPayloadMapper("CapitalReleasedFromSliceEvent", e =>
        {
            var evt = (CapitalVaultDomain.CapitalReleasedFromSliceEvent)e;
            return new CapitalVaultSchema.CapitalReleasedFromSliceEventSchema(
                evt.VaultId.Value, evt.SliceId.Value, evt.ReleasedAmount.Value,
                evt.NewSliceAvailable.Value, evt.NewSliceUsed.Value);
        });

        sink.RegisterSchema(
            "CapitalWithdrawnEvent",
            EventVersion.Default,
            typeof(CapitalVaultDomain.CapitalWithdrawnEvent),
            typeof(CapitalVaultSchema.CapitalWithdrawnEventSchema));
        sink.RegisterPayloadMapper("CapitalWithdrawnEvent", e =>
        {
            var evt = (CapitalVaultDomain.CapitalWithdrawnEvent)e;
            return new CapitalVaultSchema.CapitalWithdrawnEventSchema(
                evt.VaultId.Value, evt.SliceId.Value, evt.WithdrawnAmount.Value,
                evt.NewSliceCapacity.Value, evt.NewVaultTotal.Value);
        });
    }

    private static void RegisterLedgerEntry(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "LedgerEntryRecordedEvent",
            EventVersion.Default,
            typeof(LedgerEntryDomain.LedgerEntryRecordedEvent),
            typeof(LedgerEntrySchema.LedgerEntryRecordedEventSchema));
        sink.RegisterPayloadMapper("LedgerEntryRecordedEvent", e =>
        {
            var evt = (LedgerEntryDomain.LedgerEntryRecordedEvent)e;
            return new LedgerEntrySchema.LedgerEntryRecordedEventSchema(
                evt.EntryId.Value,
                evt.JournalId,
                evt.AccountId,
                evt.Amount.Value,
                evt.Currency.Code,
                evt.Direction.ToString(),
                evt.CreatedAt.Value);
        });
    }

    private static void RegisterLedgerObligation(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "ObligationCreatedEvent",
            EventVersion.Default,
            typeof(LedgerObligationDomain.ObligationCreatedEvent),
            typeof(LedgerObligationSchema.ObligationCreatedEventSchema));
        sink.RegisterPayloadMapper("ObligationCreatedEvent", e =>
        {
            var evt = (LedgerObligationDomain.ObligationCreatedEvent)e;
            return new LedgerObligationSchema.ObligationCreatedEventSchema(
                evt.ObligationId.Value,
                evt.CounterpartyId,
                evt.Type.ToString(),
                evt.Amount.Value,
                evt.Currency.Code);
        });

        sink.RegisterSchema(
            "ObligationFulfilledEvent",
            EventVersion.Default,
            typeof(LedgerObligationDomain.ObligationFulfilledEvent),
            typeof(LedgerObligationSchema.ObligationFulfilledEventSchema));
        sink.RegisterPayloadMapper("ObligationFulfilledEvent", e =>
        {
            var evt = (LedgerObligationDomain.ObligationFulfilledEvent)e;
            return new LedgerObligationSchema.ObligationFulfilledEventSchema(
                evt.ObligationId.Value,
                evt.SettlementId);
        });

        sink.RegisterSchema(
            "ObligationCancelledEvent",
            EventVersion.Default,
            typeof(LedgerObligationDomain.ObligationCancelledEvent),
            typeof(LedgerObligationSchema.ObligationCancelledEventSchema));
        sink.RegisterPayloadMapper("ObligationCancelledEvent", e =>
        {
            var evt = (LedgerObligationDomain.ObligationCancelledEvent)e;
            return new LedgerObligationSchema.ObligationCancelledEventSchema(
                evt.ObligationId.Value,
                evt.Reason);
        });
    }

    private static void RegisterLedgerTreasury(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "TreasuryCreatedEvent",
            EventVersion.Default,
            typeof(LedgerTreasuryDomain.TreasuryCreatedEvent),
            typeof(LedgerTreasurySchema.TreasuryCreatedEventSchema));
        sink.RegisterPayloadMapper("TreasuryCreatedEvent", e =>
        {
            var evt = (LedgerTreasuryDomain.TreasuryCreatedEvent)e;
            return new LedgerTreasurySchema.TreasuryCreatedEventSchema(
                evt.TreasuryId.Value,
                evt.Currency.Code);
        });

        sink.RegisterSchema(
            "TreasuryFundAllocatedEvent",
            EventVersion.Default,
            typeof(LedgerTreasuryDomain.TreasuryFundAllocatedEvent),
            typeof(LedgerTreasurySchema.TreasuryFundAllocatedEventSchema));
        sink.RegisterPayloadMapper("TreasuryFundAllocatedEvent", e =>
        {
            var evt = (LedgerTreasuryDomain.TreasuryFundAllocatedEvent)e;
            return new LedgerTreasurySchema.TreasuryFundAllocatedEventSchema(
                evt.TreasuryId.Value,
                evt.AllocatedAmount.Value,
                evt.NewBalance.Value);
        });

        sink.RegisterSchema(
            "TreasuryFundReleasedEvent",
            EventVersion.Default,
            typeof(LedgerTreasuryDomain.TreasuryFundReleasedEvent),
            typeof(LedgerTreasurySchema.TreasuryFundReleasedEventSchema));
        sink.RegisterPayloadMapper("TreasuryFundReleasedEvent", e =>
        {
            var evt = (LedgerTreasuryDomain.TreasuryFundReleasedEvent)e;
            return new LedgerTreasurySchema.TreasuryFundReleasedEventSchema(
                evt.TreasuryId.Value,
                evt.ReleasedAmount.Value,
                evt.NewBalance.Value);
        });
    }

    private static ContractSlice.VaultSliceType MapSlice(string slice)
    {
        return Enum.TryParse<SliceDomain.SliceType>(slice, out var parsed)
            ? (ContractSlice.VaultSliceType)(int)parsed
            : throw new InvalidOperationException($"Unknown slice value: '{slice}'.");
    }

    private static ContractSlice.VaultSliceType MapSlice(SliceDomain.SliceType slice) =>
        (ContractSlice.VaultSliceType)(int)slice;
}
