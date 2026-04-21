namespace Whycespace.Shared.Contracts.Events.Business.Agreement.ChangeControl.Approval;

public sealed record ApprovalCreatedEventSchema(Guid AggregateId);

public sealed record ApprovalApprovedEventSchema(Guid AggregateId);

public sealed record ApprovalRejectedEventSchema(Guid AggregateId);
