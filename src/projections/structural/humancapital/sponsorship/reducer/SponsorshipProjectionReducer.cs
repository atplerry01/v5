using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Sponsorship;
using Whycespace.Shared.Contracts.Structural.Humancapital.Sponsorship;

namespace Whycespace.Projections.Structural.Humancapital.Sponsorship.Reducer;

public static class SponsorshipProjectionReducer
{
    public static SponsorshipReadModel Apply(SponsorshipReadModel state, SponsorshipCreatedEventSchema e, DateTimeOffset at) =>
        state with
        {
            SponsorshipId = e.AggregateId,
            Name = e.Name,
            Kind = e.Kind,
            LastModifiedAt = at
        };
}
