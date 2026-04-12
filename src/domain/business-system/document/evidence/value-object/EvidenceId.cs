namespace Whycespace.Domain.BusinessSystem.Document.Evidence;

public readonly record struct EvidenceId
{
    public Guid Value { get; }

    public EvidenceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EvidenceId value must not be empty.", nameof(value));
        Value = value;
    }
}
