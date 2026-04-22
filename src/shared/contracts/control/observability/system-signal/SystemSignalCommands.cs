using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.Observability.SystemSignal;

public sealed record DefineSystemSignalCommand(
    Guid SignalId,
    string Name,
    string Kind,
    string Source) : IHasAggregateId
{
    public Guid AggregateId => SignalId;
}

public sealed record DeprecateSystemSignalCommand(
    Guid SignalId) : IHasAggregateId
{
    public Guid AggregateId => SignalId;
}
