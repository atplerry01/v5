using Whycespace.Shared.Contracts.Events.Business.Agreement.ChangeControl.Clause;
using DomainEvents = Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Clause;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class BusinessAgreementChangeControlClauseSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "ClauseCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ClauseCreatedEvent),
            typeof(ClauseCreatedEventSchema));

        sink.RegisterSchema(
            "ClauseActivatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ClauseActivatedEvent),
            typeof(ClauseActivatedEventSchema));

        sink.RegisterSchema(
            "ClauseSupersededEvent",
            EventVersion.Default,
            typeof(DomainEvents.ClauseSupersededEvent),
            typeof(ClauseSupersededEventSchema));

        sink.RegisterPayloadMapper("ClauseCreatedEvent", e =>
        {
            var evt = (DomainEvents.ClauseCreatedEvent)e;
            return new ClauseCreatedEventSchema(evt.ClauseId.Value, evt.ClauseType.ToString());
        });
        sink.RegisterPayloadMapper("ClauseActivatedEvent", e =>
        {
            var evt = (DomainEvents.ClauseActivatedEvent)e;
            return new ClauseActivatedEventSchema(evt.ClauseId.Value);
        });
        sink.RegisterPayloadMapper("ClauseSupersededEvent", e =>
        {
            var evt = (DomainEvents.ClauseSupersededEvent)e;
            return new ClauseSupersededEventSchema(evt.ClauseId.Value);
        });
    }
}
