using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Runtime;

public sealed record WorkflowStartCommand(Guid Id, string WorkflowName, object Payload) : IHasAggregateId
{
    public Guid AggregateId => Id;
}
