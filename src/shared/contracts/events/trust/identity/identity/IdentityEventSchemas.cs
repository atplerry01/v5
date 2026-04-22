namespace Whycespace.Shared.Contracts.Events.Trust.Identity.Identity;

public sealed record IdentityEstablishedEventSchema(Guid AggregateId, string PrincipalName, string PrincipalType);
public sealed record IdentitySuspendedEventSchema(Guid AggregateId);
public sealed record IdentityTerminatedEventSchema(Guid AggregateId);
