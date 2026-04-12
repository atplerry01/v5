namespace Whycespace.Domain.BusinessSystem.Document.SignatureRecord;

public readonly record struct SignatureRecordId
{
    public Guid Value { get; }

    public SignatureRecordId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SignatureRecordId value must not be empty.", nameof(value));

        Value = value;
    }
}
