using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Journal;

public readonly record struct JournalId
{
    public Guid Value { get; }

    public JournalId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "JournalId cannot be empty.");
        Value = value;
    }

    public static JournalId From(Guid value) => new(value);
}
