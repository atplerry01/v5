namespace Whycespace.Projections.Business.Agreement.Signature;

public sealed record SignatureView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
