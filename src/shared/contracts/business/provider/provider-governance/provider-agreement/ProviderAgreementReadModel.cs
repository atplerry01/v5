namespace Whycespace.Shared.Contracts.Business.Provider.ProviderGovernance.ProviderAgreement;

public sealed record ProviderAgreementReadModel
{
    public Guid ProviderAgreementId { get; init; }
    public Guid ProviderId { get; init; }
    public Guid? ContractId { get; init; }
    public DateTimeOffset? EffectiveStartsAt { get; init; }
    public DateTimeOffset? EffectiveEndsAt { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset? SuspendedAt { get; init; }
    public DateTimeOffset? TerminatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
