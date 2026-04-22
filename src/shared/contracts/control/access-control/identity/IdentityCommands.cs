using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.AccessControl.Identity;

public sealed record RegisterIdentityCommand(
    Guid IdentityId,
    string Name,
    string Kind) : IHasAggregateId
{
    public Guid AggregateId => IdentityId;
}

public sealed record SuspendIdentityCommand(
    Guid IdentityId,
    string Reason) : IHasAggregateId
{
    public Guid AggregateId => IdentityId;
}

public sealed record DeactivateIdentityCommand(
    Guid IdentityId) : IHasAggregateId
{
    public Guid AggregateId => IdentityId;
}
