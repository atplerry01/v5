using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderCoverage;

public sealed record ProviderCoverageCreatedEvent(ProviderCoverageId ProviderCoverageId, ProviderRef Provider);
