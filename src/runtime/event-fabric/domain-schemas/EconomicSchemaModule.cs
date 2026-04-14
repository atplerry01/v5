using Whycespace.Shared.Contracts.Events.Economic.Ledger.Journal;
using Whycespace.Shared.Contracts.Events.Economic.Ledger.Ledger;
using Whycespace.Shared.Contracts.Events.Economic.Revenue.Distribution;
using Whycespace.Shared.Contracts.Events.Economic.Revenue.Payout;
using Whycespace.Shared.Contracts.Events.Economic.Revenue.Revenue;
using Whycespace.Shared.Contracts.Events.Economic.Vault.Account;
using ContractSlice = Whycespace.Shared.Contracts.Events.Economic.Vault.Account;
using JournalDomain = Whycespace.Domain.EconomicSystem.Ledger.Journal;
using LedgerDomain = Whycespace.Domain.EconomicSystem.Ledger.Ledger;
using RevenueDomain = Whycespace.Domain.EconomicSystem.Revenue.Revenue;
using DistributionDomain = Whycespace.Domain.EconomicSystem.Revenue.Distribution;
using PayoutDomain = Whycespace.Domain.EconomicSystem.Revenue.Payout;
using VaultDomain = Whycespace.Domain.EconomicSystem.Vault.Account;
using SliceDomain = Whycespace.Domain.EconomicSystem.Vault.Slice;

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
