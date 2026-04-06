namespace Whycespace.Projections.Business.Document.SignatureRecord;

public sealed record SignatureRecordView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
