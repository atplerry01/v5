namespace Whycespace.Shared.Contracts.Events.Business.Provider.ProviderGovernance.ProviderAgreement;

public sealed record ProviderAgreementCreatedEventSchema(
    Guid AggregateId,
    Guid ProviderId,
    Guid? ContractId,
    DateTimeOffset? EffectiveStartsAt,
    DateTimeOffset? EffectiveEndsAt);

public sealed record ProviderAgreementActivatedEventSchema(
    Guid AggregateId,
    DateTimeOffset EffectiveStartsAt,
    DateTimeOffset? EffectiveEndsAt);

public sealed record ProviderAgreementSuspendedEventSchema(
    Guid AggregateId,
    DateTimeOffset SuspendedAt);

public sealed record ProviderAgreementTerminatedEventSchema(
    Guid AggregateId,
    DateTimeOffset TerminatedAt);
