namespace Whycespace.Shared.Contracts.Events.Business.Provider.ProviderCore.ProviderCapability;

public sealed record ProviderCapabilityCreatedEventSchema(
    Guid AggregateId,
    Guid ProviderId,
    string Code,
    string Name);

public sealed record ProviderCapabilityUpdatedEventSchema(
    Guid AggregateId,
    string Name);

public sealed record ProviderCapabilityActivatedEventSchema(Guid AggregateId);

public sealed record ProviderCapabilityArchivedEventSchema(Guid AggregateId);
