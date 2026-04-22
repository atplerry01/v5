namespace Whycespace.Shared.Contracts.Events.Trust.Identity.Federation;

public sealed record FederationEstablishedEventSchema(Guid AggregateId, Guid IdentityReference, string FederatedProvider, string FederationType, DateTimeOffset EstablishedAt);
public sealed record FederationSuspendedEventSchema(Guid AggregateId);
public sealed record FederationTerminatedEventSchema(Guid AggregateId);
