using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Structural.Humancapital.Operator;

public sealed record CreateOperatorCommand(
    Guid OperatorId,
    string Name,
    string Kind) : IHasAggregateId
{
    public Guid AggregateId => OperatorId;
}
