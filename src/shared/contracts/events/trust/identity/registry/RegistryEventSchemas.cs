namespace Whycespace.Shared.Contracts.Events.Trust.Identity.Registry;

public sealed record RegistrationInitiatedEventSchema(Guid AggregateId, string Email, string RegistrationType, DateTimeOffset InitiatedAt);
public sealed record RegistrationVerifiedEventSchema(Guid AggregateId);
public sealed record RegistrationActivatedEventSchema(Guid AggregateId);
public sealed record RegistrationRejectedEventSchema(Guid AggregateId, string Reason);
public sealed record RegistrationLockedEventSchema(Guid AggregateId, string Reason);
