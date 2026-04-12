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

    private JournalEntry() { }

    internal static JournalEntry Create(
        Guid entryId,
        Guid accountId,
        Amount amount,
        Currency currency,
        BookingDirection direction)
    {
        return new JournalEntry
        {
            EntryId = entryId,
            AccountId = accountId,
            Amount = amount,
            Currency = currency,
            Direction = direction
        };
    }
}
