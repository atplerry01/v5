namespace Whycespace.Domain.BusinessSystem.Shared.Reference;

// Canonical reference to a business customer from outside the customer leaf.
// Shared across leaves to avoid per-leaf duplication; aggregate types are
// still NOT imported across bounded contexts.
public readonly record struct CustomerRef
{
    public Guid Value { get; }

    public CustomerRef(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CustomerRef value must not be empty.", nameof(value));

        Value = value;
    }
}
