namespace Whycespace.Projections.Business.Document.Record;

public sealed record RecordView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
