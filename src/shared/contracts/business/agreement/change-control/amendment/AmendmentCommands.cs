using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Amendment;

public sealed record CreateAmendmentCommand(Guid AmendmentId, Guid TargetId) : IHasAggregateId
{
    public Guid AggregateId => AmendmentId;
}

public sealed record ApplyAmendmentCommand(Guid AmendmentId) : IHasAggregateId
{
    public Guid AggregateId => AmendmentId;
}

public sealed record RevertAmendmentCommand(Guid AmendmentId) : IHasAggregateId
{
    public Guid AggregateId => AmendmentId;
}
