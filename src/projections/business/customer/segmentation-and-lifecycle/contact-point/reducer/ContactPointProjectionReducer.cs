using Whycespace.Shared.Contracts.Business.Customer.SegmentationAndLifecycle.ContactPoint;
using Whycespace.Shared.Contracts.Events.Business.Customer.SegmentationAndLifecycle.ContactPoint;

namespace Whycespace.Projections.Business.Customer.SegmentationAndLifecycle.ContactPoint.Reducer;

public static class ContactPointProjectionReducer
{
    public static ContactPointReadModel Apply(ContactPointReadModel state, ContactPointCreatedEventSchema e) =>
        state with
        {
            ContactPointId = e.AggregateId,
            CustomerId = e.CustomerId,
            Kind = e.Kind,
            Value = e.Value,
            Status = "Draft",
            IsPreferred = false
        };

    public static ContactPointReadModel Apply(ContactPointReadModel state, ContactPointUpdatedEventSchema e) =>
        state with
        {
            ContactPointId = e.AggregateId,
            Value = e.Value
        };

    public static ContactPointReadModel Apply(ContactPointReadModel state, ContactPointActivatedEventSchema e) =>
        state with
        {
            ContactPointId = e.AggregateId,
            Status = "Active"
        };

    public static ContactPointReadModel Apply(ContactPointReadModel state, ContactPointPreferredSetEventSchema e) =>
        state with
        {
            ContactPointId = e.AggregateId,
            IsPreferred = e.IsPreferred
        };

    public static ContactPointReadModel Apply(ContactPointReadModel state, ContactPointArchivedEventSchema e) =>
        state with
        {
            ContactPointId = e.AggregateId,
            Status = "Archived",
            IsPreferred = false
        };
}
