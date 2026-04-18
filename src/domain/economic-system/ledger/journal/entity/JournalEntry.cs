using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.SharedKernel.Primitive.Money;

namespace Whycespace.Domain.EconomicSystem.Ledger.Journal;

public sealed class JournalEntry : Entity
{
    public Guid EntryId { get; private set; }
    public Guid AccountId { get; private set; }
    public Amount Amount { get; private set; }
    public Currency Currency { get; private set; }
    public BookingDirection Direction { get; private set; }

    // Phase 6 T6.1 — FX rate snapshot. Non-null only when the producing
    // transaction crossed FxLockStep. Ledger read path consumes these
    // fields directly; no exchange-rate resolver call at query time.
    public Guid? FxRateId { get; private set; }
    public decimal? FxRate { get; private set; }

    private JournalEntry() { }

    internal static JournalEntry Create(
        Guid entryId,
        Guid accountId,
        Amount amount,
        Currency currency,
        BookingDirection direction,
        Guid? fxRateId = null,
        decimal? fxRate = null)
    {
        return new JournalEntry
        {
            EntryId = entryId,
            AccountId = accountId,
            Amount = amount,
            Currency = currency,
            Direction = direction,
            FxRateId = fxRateId,
            FxRate = fxRate,
        };
    }
}
