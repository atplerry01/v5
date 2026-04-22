using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Trust.Identity.Consent;

public sealed record GrantConsentCommand(
    Guid ConsentId,
    Guid IdentityReference,
    string ConsentScope,
    string ConsentPurpose,
    DateTimeOffset GrantedAt) : IHasAggregateId
{
    public Guid AggregateId => ConsentId;
}

public sealed record RevokeConsentCommand(Guid ConsentId) : IHasAggregateId
{
    public Guid AggregateId => ConsentId;
}

public sealed record ExpireConsentCommand(Guid ConsentId) : IHasAggregateId
{
    public Guid AggregateId => ConsentId;
}
