namespace Whycespace.Domain.BusinessSystem.Document.Version;

public readonly record struct VersionId
{
    public Guid Value { get; }

    public VersionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("VersionId value must not be empty.", nameof(value));

        Value = value;
    }
}