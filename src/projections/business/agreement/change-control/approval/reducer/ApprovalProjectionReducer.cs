using Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Approval;
using Whycespace.Shared.Contracts.Events.Business.Agreement.ChangeControl.Approval;

namespace Whycespace.Projections.Business.Agreement.ChangeControl.Approval.Reducer;

public static class ApprovalProjectionReducer
{
    public static ApprovalReadModel Apply(ApprovalReadModel state, ApprovalCreatedEventSchema e) =>
        state with
        {
            ApprovalId = e.AggregateId,
            Status = "Pending",
            CreatedAt = DateTimeOffset.MinValue,
            LastUpdatedAt = DateTimeOffset.MinValue
        };

    public static ApprovalReadModel Apply(ApprovalReadModel state, ApprovalApprovedEventSchema e) =>
        state with
        {
            ApprovalId = e.AggregateId,
            Status = "Approved"
        };

    public static ApprovalReadModel Apply(ApprovalReadModel state, ApprovalRejectedEventSchema e) =>
        state with
        {
            ApprovalId = e.AggregateId,
            Status = "Rejected"
        };
}
