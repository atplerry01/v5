namespace Whycespace.Projections.Business.Integration.Export;

public sealed record ExportView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
