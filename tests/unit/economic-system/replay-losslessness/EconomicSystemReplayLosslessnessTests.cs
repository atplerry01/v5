using Whycespace.Domain.EconomicSystem.Capital.Pool;
using Whycespace.Domain.EconomicSystem.Capital.Account;
using Whycespace.Domain.EconomicSystem.Capital.Vault;
using Whycespace.Domain.EconomicSystem.Ledger.Entry;
using Whycespace.Domain.EconomicSystem.Ledger.Journal;
using Whycespace.Domain.EconomicSystem.Ledger.Treasury;
using Whycespace.Domain.EconomicSystem.Ledger.Obligation;
using Whycespace.Domain.EconomicSystem.Transaction.Instruction;
using Whycespace.Domain.EconomicSystem.Transaction.Transaction;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.EconomicSystem.ReplayLosslessness;

/// <summary>
/// INV-REPLAY-LOSSLESS-VALUEOBJECT-01 — economic-system
/// Verifies that LoadFromHistory produces structurally identical aggregate state
/// to direct factory construction — all VO fields survive the event round-trip.
/// </summary>
public sealed class EconomicSystemReplayLosslessnessTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));
    private static readonly Currency Usd = new("USD");

    // ── CapitalPoolAggregate ─────────────────────────────────────────────────

    [Fact]
    public void CapitalPoolAggregate_Replay_PreservesAllVoFields()
    {
        var id = new PoolId(IdGen.Generate("LS:pool:id"));

        var direct = CapitalPoolAggregate.Create(id, Usd, BaseTime);

        var replayed = (CapitalPoolAggregate)Activator.CreateInstance(typeof(CapitalPoolAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(new object[]
        {
            new PoolCreatedEvent(id, Usd, BaseTime)
        });

        Assert.Equal(direct.PoolId, replayed.PoolId);
        Assert.Equal(direct.Currency.Code, replayed.Currency.Code);
        Assert.Equal(direct.TotalCapital.Value, replayed.TotalCapital.Value);
    }

    // ── TreasuryAggregate ────────────────────────────────────────────────────

    [Fact]
    public void TreasuryAggregate_Replay_PreservesAllVoFields()
    {
        var id = new TreasuryId(IdGen.Generate("LS:treasury:id"));

        var direct = TreasuryAggregate.Create(id, Usd, BaseTime);

        var replayed = (TreasuryAggregate)Activator.CreateInstance(typeof(TreasuryAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(new object[]
        {
            new TreasuryCreatedEvent(id, Usd, BaseTime)
        });

        Assert.Equal(direct.TreasuryId, replayed.TreasuryId);
        Assert.Equal(direct.Currency.Code, replayed.Currency.Code);
        Assert.Equal(direct.Balance.Value, replayed.Balance.Value);
    }

    // ── ObligationAggregate ──────────────────────────────────────────────────

    [Fact]
    public void ObligationAggregate_Replay_PreservesAllVoFields()
    {
        var id = new ObligationId(IdGen.Generate("LS:obligation:id"));
        var counterparty = new CounterpartyRef(IdGen.Generate("LS:obligation:counterparty"));

        var direct = ObligationAggregate.Create(id, counterparty, ObligationType.Payable, new Amount(1000m), Usd, BaseTime);

        var replayed = (ObligationAggregate)Activator.CreateInstance(typeof(ObligationAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(new object[]
        {
            new ObligationCreatedEvent(id, counterparty.Value, ObligationType.Payable, new Amount(1000m), Usd, BaseTime)
        });

        Assert.Equal(direct.ObligationId, replayed.ObligationId);
        Assert.Equal(direct.Type, replayed.Type);
        Assert.Equal(direct.Amount.Value, replayed.Amount.Value);
        Assert.Equal(direct.Currency.Code, replayed.Currency.Code);
        Assert.Equal(direct.Status, replayed.Status);
    }

    // ── LedgerEntryAggregate ─────────────────────────────────────────────────

    [Fact]
    public void LedgerEntryAggregate_Replay_PreservesAllVoFields()
    {
        var id = new EntryId(IdGen.Generate("LS:entry:id"));
        var journalId = new JournalId(IdGen.Generate("LS:entry:journal"));
        var accountId = new AccountId(IdGen.Generate("LS:entry:account"));

        var direct = LedgerEntryAggregate.Record(id, journalId, accountId, new Amount(500m), Usd, EntryDirection.Debit, BaseTime);

        var replayed = (LedgerEntryAggregate)Activator.CreateInstance(typeof(LedgerEntryAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(new object[]
        {
            new LedgerEntryRecordedEvent(id, journalId.Value, accountId.Value, new Amount(500m), Usd, EntryDirection.Debit, BaseTime)
        });

        Assert.Equal(direct.EntryId, replayed.EntryId);
        Assert.Equal(direct.Amount.Value, replayed.Amount.Value);
        Assert.Equal(direct.Direction, replayed.Direction);
        Assert.Equal(direct.IsRecorded, replayed.IsRecorded);
    }

    // ── TransactionInstructionAggregate ──────────────────────────────────────

    [Fact]
    public void TransactionInstructionAggregate_Replay_PreservesAllVoFields()
    {
        var id = new InstructionId(IdGen.Generate("LS:instruction:id"));
        var from = new AccountId(IdGen.Generate("LS:instruction:from"));
        var to = new AccountId(IdGen.Generate("LS:instruction:to"));

        var direct = TransactionInstructionAggregate.CreateInstruction(
            id, from, to, new Amount(750m), Usd, InstructionType.Transfer, BaseTime);

        var replayed = (TransactionInstructionAggregate)Activator.CreateInstance(typeof(TransactionInstructionAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(new object[]
        {
            new TransactionInstructionCreatedEvent(id, from.Value, to.Value, new Amount(750m), Usd, InstructionType.Transfer, BaseTime)
        });

        Assert.Equal(direct.InstructionId, replayed.InstructionId);
        Assert.Equal(direct.Amount.Value, replayed.Amount.Value);
        Assert.Equal(direct.Currency.Code, replayed.Currency.Code);
        Assert.Equal(direct.Type, replayed.Type);
        Assert.Equal(direct.Status, replayed.Status);
    }

    // ── VaultAggregate ───────────────────────────────────────────────────────

    [Fact]
    public void VaultAggregate_Replay_PreservesAllVoFields()
    {
        var id = new VaultId(IdGen.Generate("LS:vault:id"));
        var owner = new OwnerId(IdGen.Generate("LS:vault:owner"));

        var direct = VaultAggregate.Create(id, owner, Usd, BaseTime);

        var replayed = (VaultAggregate)Activator.CreateInstance(typeof(VaultAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(new object[]
        {
            new VaultCreatedEvent(id, owner.Value, Usd, BaseTime)
        });

        Assert.Equal(direct.VaultId, replayed.VaultId);
        Assert.Equal(direct.OwnerId, replayed.OwnerId);
        Assert.Equal(direct.Currency.Code, replayed.Currency.Code);
        Assert.Equal(direct.TotalStored.Value, replayed.TotalStored.Value);
    }
}
