using Whycespace.Shared.Contracts.Events.Business.Order.OrderChange.Amendment;
using DomainEvents = Whycespace.Domain.BusinessSystem.Order.OrderChange.Amendment;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/order/order-change/amendment domain.
///
/// Distinct from <see cref="BusinessAgreementChangeControlAmendmentSchemaModule"/>
/// — event names are registered with the <c>Amendment</c> prefix the same way,
/// but the CLR types live in a different namespace
/// (<c>Whycespace.Domain.BusinessSystem.Order.OrderChange.Amendment</c>) and
/// the payload schemas live in
/// <c>Whycespace.Shared.Contracts.Events.Business.Order.OrderChange.Amendment</c>.
/// The <see cref="EventSchemaRegistry"/> keys on the domain CLR type, so the
/// two modules coexist without collision.
/// </summary>
public sealed class BusinessOrderOrderChangeAmendmentSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "AmendmentRequestedEvent",
            EventVersion.Default,
            typeof(DomainEvents.AmendmentRequestedEvent),
            typeof(AmendmentRequestedEventSchema));

        sink.RegisterSchema(
            "AmendmentAcceptedEvent",
            EventVersion.Default,
            typeof(DomainEvents.AmendmentAcceptedEvent),
            typeof(AmendmentAcceptedEventSchema));

        sink.RegisterSchema(
            "AmendmentAppliedEvent",
            EventVersion.Default,
            typeof(DomainEvents.AmendmentAppliedEvent),
            typeof(AmendmentAppliedEventSchema));

        sink.RegisterSchema(
            "AmendmentRejectedEvent",
            EventVersion.Default,
            typeof(DomainEvents.AmendmentRejectedEvent),
            typeof(AmendmentRejectedEventSchema));

        sink.RegisterSchema(
            "AmendmentCancelledEvent",
            EventVersion.Default,
            typeof(DomainEvents.AmendmentCancelledEvent),
            typeof(AmendmentCancelledEventSchema));

        sink.RegisterPayloadMapper("AmendmentRequestedEvent", e =>
        {
            var evt = (DomainEvents.AmendmentRequestedEvent)e;
            return new AmendmentRequestedEventSchema(
                evt.AmendmentId.Value,
                evt.Order.Value,
                evt.Reason.Value,
                evt.RequestedAt);
        });
        sink.RegisterPayloadMapper("AmendmentAcceptedEvent", e =>
        {
            var evt = (DomainEvents.AmendmentAcceptedEvent)e;
            return new AmendmentAcceptedEventSchema(evt.AmendmentId.Value, evt.AcceptedAt);
        });
        sink.RegisterPayloadMapper("AmendmentAppliedEvent", e =>
        {
            var evt = (DomainEvents.AmendmentAppliedEvent)e;
            return new AmendmentAppliedEventSchema(evt.AmendmentId.Value, evt.AppliedAt);
        });
        sink.RegisterPayloadMapper("AmendmentRejectedEvent", e =>
        {
            var evt = (DomainEvents.AmendmentRejectedEvent)e;
            return new AmendmentRejectedEventSchema(evt.AmendmentId.Value, evt.RejectedAt);
        });
        sink.RegisterPayloadMapper("AmendmentCancelledEvent", e =>
        {
            var evt = (DomainEvents.AmendmentCancelledEvent)e;
            return new AmendmentCancelledEventSchema(evt.AmendmentId.Value, evt.CancelledAt);
        });
    }
}
