using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Customer.SegmentationAndLifecycle.Segment;

public sealed record CreateSegmentCommand(
    Guid SegmentId,
    string Code,
    string Name,
    string Type,
    string Criteria) : IHasAggregateId
{
    public Guid AggregateId => SegmentId;
}

public sealed record UpdateSegmentCommand(
    Guid SegmentId,
    string Name,
    string Criteria) : IHasAggregateId
{
    public Guid AggregateId => SegmentId;
}

public sealed record ActivateSegmentCommand(Guid SegmentId) : IHasAggregateId
{
    public Guid AggregateId => SegmentId;
}

public sealed record ArchiveSegmentCommand(Guid SegmentId) : IHasAggregateId
{
    public Guid AggregateId => SegmentId;
}
