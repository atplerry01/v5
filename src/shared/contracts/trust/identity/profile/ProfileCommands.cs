using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Trust.Identity.Profile;

public sealed record CreateProfileCommand(
    Guid ProfileId,
    Guid IdentityReference,
    string DisplayName,
    string ProfileType,
    DateTimeOffset CreatedAt) : IHasAggregateId
{
    public Guid AggregateId => ProfileId;
}

public sealed record ActivateProfileCommand(Guid ProfileId) : IHasAggregateId
{
    public Guid AggregateId => ProfileId;
}

public sealed record DeactivateProfileCommand(Guid ProfileId) : IHasAggregateId
{
    public Guid AggregateId => ProfileId;
}
