using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Revenue.Revenue;

public sealed record RecordRevenueCommand(
    Guid RevenueId,
    string SpvId,
    decimal Amount,
    string Currency,
    string SourceRef) : IHasAggregateId
{
    public Guid AggregateId => RevenueId;
}
