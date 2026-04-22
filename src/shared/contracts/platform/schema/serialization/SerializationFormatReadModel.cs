namespace Whycespace.Shared.Contracts.Platform.Schema.Serialization;

public sealed record SerializationFormatReadModel
{
    public Guid SerializationFormatId { get; init; }
    public string FormatName { get; init; } = string.Empty;
    public string Encoding { get; init; } = string.Empty;
    public Guid? SchemaRef { get; init; }
    public string RoundTripGuarantee { get; init; } = string.Empty;
    public int FormatVersion { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset LastModifiedAt { get; init; }
}
