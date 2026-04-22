namespace Whycespace.Shared.Contracts.Events.Control.AccessControl.Identity;

public sealed record IdentityRegisteredEventSchema(
    Guid AggregateId,
    string Name,
    string Kind);

public sealed record IdentitySuspendedEventSchema(
    Guid AggregateId,
    string Reason);

public sealed record IdentityDeactivatedEventSchema(
    Guid AggregateId);
