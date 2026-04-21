namespace Whycespace.Shared.Contracts.Events.Business.Agreement.ChangeControl.Amendment;

public sealed record AmendmentCreatedEventSchema(Guid AggregateId, Guid TargetId);

public sealed record AmendmentAppliedEventSchema(Guid AggregateId);

public sealed record AmendmentRevertedEventSchema(Guid AggregateId);
