namespace Whycespace.Shared.Contracts.Events.Trust.Identity.IdentityGraph;

public sealed record IdentityGraphInitializedEventSchema(Guid AggregateId, Guid PrimaryIdentityReference, string GraphContext, DateTimeOffset InitializedAt);
public sealed record IdentityGraphArchivedEventSchema(Guid AggregateId);
