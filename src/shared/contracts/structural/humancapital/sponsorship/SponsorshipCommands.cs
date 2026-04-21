using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Structural.Humancapital.Sponsorship;

public sealed record CreateSponsorshipCommand(
    Guid SponsorshipId,
    string Name,
    string Kind) : IHasAggregateId
{
    public Guid AggregateId => SponsorshipId;
}
