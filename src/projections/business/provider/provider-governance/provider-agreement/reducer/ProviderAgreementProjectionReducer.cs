using Whycespace.Shared.Contracts.Business.Provider.ProviderGovernance.ProviderAgreement;
using Whycespace.Shared.Contracts.Events.Business.Provider.ProviderGovernance.ProviderAgreement;

namespace Whycespace.Projections.Business.Provider.ProviderGovernance.ProviderAgreement.Reducer;

public static class ProviderAgreementProjectionReducer
{
    public static ProviderAgreementReadModel Apply(ProviderAgreementReadModel state, ProviderAgreementCreatedEventSchema e) =>
        state with
        {
            ProviderAgreementId = e.AggregateId,
            ProviderId = e.ProviderId,
            ContractId = e.ContractId,
            EffectiveStartsAt = e.EffectiveStartsAt,
            EffectiveEndsAt = e.EffectiveEndsAt,
            Status = "Draft"
        };

    public static ProviderAgreementReadModel Apply(ProviderAgreementReadModel state, ProviderAgreementActivatedEventSchema e) =>
        state with
        {
            ProviderAgreementId = e.AggregateId,
            EffectiveStartsAt = e.EffectiveStartsAt,
            EffectiveEndsAt = e.EffectiveEndsAt,
            Status = "Active"
        };

    public static ProviderAgreementReadModel Apply(ProviderAgreementReadModel state, ProviderAgreementSuspendedEventSchema e) =>
        state with
        {
            ProviderAgreementId = e.AggregateId,
            Status = "Suspended",
            SuspendedAt = e.SuspendedAt
        };

    public static ProviderAgreementReadModel Apply(ProviderAgreementReadModel state, ProviderAgreementTerminatedEventSchema e) =>
        state with
        {
            ProviderAgreementId = e.AggregateId,
            Status = "Terminated",
            TerminatedAt = e.TerminatedAt
        };
}
