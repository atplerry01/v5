namespace Whycespace.Projections.Business.Agreement.Contract;

public sealed record ContractView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
