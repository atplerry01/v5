namespace Whycespace.Shared.Contracts.Events.Trust.Access.Session;

public sealed record SessionOpenedEventSchema(Guid AggregateId, Guid IdentityReference, string SessionContext, DateTimeOffset OpenedAt);
public sealed record SessionExpiredEventSchema(Guid AggregateId);
public sealed record SessionTerminatedEventSchema(Guid AggregateId);
