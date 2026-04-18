using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Ledger;

public sealed class PostedJournalReference : Entity
{
    public Guid JournalId { get; private set; }
    public Amount TotalDebit { get; private set; }
    public Amount TotalCredit { get; private set; }
    public Timestamp PostedAt { get; private set; }

    private PostedJournalReference() { }

    internal static PostedJournalReference Create(
        Guid journalId,
        Amount totalDebit,
        Amount totalCredit,
        Timestamp postedAt)
    {
        Guard.Against(journalId == Guid.Empty, "Journal reference cannot be empty.");

        return new PostedJournalReference
        {
            JournalId = journalId,
            TotalDebit = totalDebit,
            TotalCredit = totalCredit,
            PostedAt = postedAt
        };
    }
}
