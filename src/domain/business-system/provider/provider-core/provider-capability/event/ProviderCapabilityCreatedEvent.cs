using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderCapability;

public sealed record ProviderCapabilityCreatedEvent(
    ProviderCapabilityId ProviderCapabilityId,
    ProviderRef Provider,
    CapabilityCode Code,
    CapabilityName Name);
