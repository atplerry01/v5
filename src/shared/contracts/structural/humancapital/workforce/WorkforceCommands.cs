using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Structural.Humancapital.Workforce;

public sealed record CreateWorkforceCommand(
    Guid WorkforceId,
    string Name,
    string Kind) : IHasAggregateId
{
    public Guid AggregateId => WorkforceId;
}
