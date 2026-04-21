namespace Whycespace.Shared.Contracts.Events.Business.Customer.IdentityAndProfile.Profile;

public sealed record ProfileCreatedEventSchema(
    Guid AggregateId,
    Guid CustomerId,
    string DisplayName);

public sealed record ProfileRenamedEventSchema(Guid AggregateId, string DisplayName);

public sealed record ProfileDescriptorSetEventSchema(Guid AggregateId, string Key, string Value);

public sealed record ProfileDescriptorRemovedEventSchema(Guid AggregateId, string Key);

public sealed record ProfileActivatedEventSchema(Guid AggregateId);

public sealed record ProfileArchivedEventSchema(Guid AggregateId);
