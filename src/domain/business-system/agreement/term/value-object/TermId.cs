namespace Whycespace.Domain.BusinessSystem.Agreement.Term;

public readonly record struct TermId
{
    public Guid Value { get; }

    public TermId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TermId value must not be empty.", nameof(value));

        Value = value;
    }
}
