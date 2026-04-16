using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Reconciliation.Discrepancy;

public sealed record DetectDiscrepancyCommand(
    Guid DiscrepancyId,
    Guid ProcessReference,
    string Source,
    decimal ExpectedValue,
    decimal ActualValue,
    decimal Difference,
    DateTimeOffset DetectedAt) : IHasAggregateId
{
    public Guid AggregateId => DiscrepancyId;
}

public sealed record InvestigateDiscrepancyCommand(Guid DiscrepancyId) : IHasAggregateId
{
    public Guid AggregateId => DiscrepancyId;
}

public sealed record ResolveDiscrepancyCommand(Guid DiscrepancyId, string Resolution) : IHasAggregateId
{
    public Guid AggregateId => DiscrepancyId;
}
