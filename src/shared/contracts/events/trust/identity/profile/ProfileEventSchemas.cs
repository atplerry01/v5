namespace Whycespace.Shared.Contracts.Events.Trust.Identity.Profile;

public sealed record ProfileCreatedEventSchema(Guid AggregateId, Guid IdentityReference, string DisplayName, string ProfileType, DateTimeOffset CreatedAt);
public sealed record ProfileActivatedEventSchema(Guid AggregateId);
public sealed record ProfileDeactivatedEventSchema(Guid AggregateId);
