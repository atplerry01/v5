namespace Whycespace.Domain.BusinessSystem.Document.Record;

public readonly record struct RecordId
{
    public Guid Value { get; }

    public RecordId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RecordId value must not be empty.", nameof(value));
        Value = value;
    }
}
