namespace Whycespace.Domain.BusinessSystem.Agreement.Clause;

public readonly record struct ClauseId
{
    public Guid Value { get; }

    public ClauseId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ClauseId value must not be empty.", nameof(value));

        Value = value;
    }
}
