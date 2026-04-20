using Whycespace.Domain.BusinessSystem.Shared.Reference;

using Whycespace.Domain.BusinessSystem.Shared.Time;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderGovernance.ProviderAgreement;

public sealed record ProviderAgreementCreatedEvent(
    ProviderAgreementId ProviderAgreementId,
    ProviderRef Provider,
    ContractRef? Contract,
    TimeWindow? Effective);
