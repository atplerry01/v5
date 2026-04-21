using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Customer.SegmentationAndLifecycle.ContactPoint;

public sealed record CreateContactPointCommand(
    Guid ContactPointId,
    Guid CustomerId,
    string Kind,
    string Value) : IHasAggregateId
{
    public Guid AggregateId => ContactPointId;
}

public sealed record UpdateContactPointCommand(
    Guid ContactPointId,
    string Value) : IHasAggregateId
{
    public Guid AggregateId => ContactPointId;
}

public sealed record ActivateContactPointCommand(Guid ContactPointId) : IHasAggregateId
{
    public Guid AggregateId => ContactPointId;
}

public sealed record SetContactPointPreferredCommand(
    Guid ContactPointId,
    bool IsPreferred) : IHasAggregateId
{
    public Guid AggregateId => ContactPointId;
}

public sealed record ArchiveContactPointCommand(Guid ContactPointId) : IHasAggregateId
{
    public Guid AggregateId => ContactPointId;
}
