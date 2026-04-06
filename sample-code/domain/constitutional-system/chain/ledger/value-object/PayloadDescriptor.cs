namespace Whycespace.Domain.ConstitutionalSystem.Chain.Ledger;

public sealed class PayloadDescriptor : ValueObject
{
    public string PayloadType { get; }
    public string PayloadId { get; }

    private PayloadDescriptor(string payloadType, string payloadId)
    {
        Guard.AgainstEmpty(payloadType, nameof(payloadType));
        Guard.AgainstEmpty(payloadId, nameof(payloadId));
        PayloadType = payloadType;
        PayloadId = payloadId;
    }

    public static PayloadDescriptor From(string payloadType, string payloadId) => new(payloadType, payloadId);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return PayloadType;
        yield return PayloadId;
    }
}
