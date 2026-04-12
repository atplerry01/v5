using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Entry;

public readonly record struct EntryId
{
    public Guid Value { get; }

    public EntryId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "EntryId cannot be empty.");
        Value = value;
    }

    public static EntryId From(Guid value) => new(value);
}
