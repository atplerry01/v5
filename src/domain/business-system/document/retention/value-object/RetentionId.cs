namespace Whycespace.Domain.BusinessSystem.Document.Retention;

public readonly record struct RetentionId
{
    public Guid Value { get; }

    public RetentionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RetentionId value must not be empty.", nameof(value));

        Value = value;
    }
}
