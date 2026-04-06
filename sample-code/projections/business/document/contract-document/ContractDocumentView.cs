namespace Whycespace.Projections.Business.Document.ContractDocument;

public sealed record ContractDocumentView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
