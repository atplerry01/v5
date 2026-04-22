namespace Whycespace.Shared.Contracts.Events.Trust.Access.Request;

public sealed record RequestSubmittedEventSchema(Guid AggregateId, Guid PrincipalReference, string RequestType, string RequestScope, DateTimeOffset SubmittedAt);
public sealed record RequestApprovedEventSchema(Guid AggregateId);
public sealed record RequestDeniedEventSchema(Guid AggregateId, string Reason);
public sealed record RequestWithdrawnEventSchema(Guid AggregateId);
