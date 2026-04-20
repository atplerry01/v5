using Whycespace.Domain.BusinessSystem.Shared.Time;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderGovernance.ProviderAgreement;

public sealed record ProviderAgreementActivatedEvent(ProviderAgreementId ProviderAgreementId, TimeWindow Effective);
