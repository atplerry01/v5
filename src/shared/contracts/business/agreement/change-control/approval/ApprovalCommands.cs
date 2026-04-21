using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Approval;

public sealed record CreateApprovalCommand(Guid ApprovalId) : IHasAggregateId
{
    public Guid AggregateId => ApprovalId;
}

public sealed record ApproveApprovalCommand(Guid ApprovalId) : IHasAggregateId
{
    public Guid AggregateId => ApprovalId;
}

public sealed record RejectApprovalCommand(Guid ApprovalId) : IHasAggregateId
{
    public Guid AggregateId => ApprovalId;
}
