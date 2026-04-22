namespace Whycespace.Domain.PlatformSystem.Schema.Serialization;

public readonly record struct RoundTripGuarantee
{
    public static readonly RoundTripGuarantee Lossless = new("Lossless");
    public static readonly RoundTripGuarantee LossyWithDocumentedFields = new("LossyWithDocumentedFields");

    public string Value { get; }

    private RoundTripGuarantee(string value) => Value = value;
}
