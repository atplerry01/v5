using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Trust.Access.Session;

public sealed record OpenSessionCommand(
    Guid SessionId,
    Guid IdentityReference,
    string SessionContext,
    DateTimeOffset OpenedAt) : IHasAggregateId
{
    public Guid AggregateId => SessionId;
}

public sealed record ExpireSessionCommand(Guid SessionId) : IHasAggregateId
{
    public Guid AggregateId => SessionId;
}

public sealed record TerminateSessionCommand(Guid SessionId) : IHasAggregateId
{
    public Guid AggregateId => SessionId;
}
