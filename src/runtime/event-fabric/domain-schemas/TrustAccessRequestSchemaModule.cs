using Whycespace.Shared.Contracts.Events.Trust.Access.Request;
using DomainEvents = Whycespace.Domain.TrustSystem.Access.Request;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class TrustAccessRequestSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("RequestSubmittedEvent", EventVersion.Default,
            typeof(DomainEvents.RequestSubmittedEvent), typeof(RequestSubmittedEventSchema));
        sink.RegisterSchema("RequestApprovedEvent", EventVersion.Default,
            typeof(DomainEvents.RequestApprovedEvent), typeof(RequestApprovedEventSchema));
        sink.RegisterSchema("RequestDeniedEvent", EventVersion.Default,
            typeof(DomainEvents.RequestDeniedEvent), typeof(RequestDeniedEventSchema));
        sink.RegisterSchema("RequestWithdrawnEvent", EventVersion.Default,
            typeof(DomainEvents.RequestWithdrawnEvent), typeof(RequestWithdrawnEventSchema));

        sink.RegisterPayloadMapper("RequestSubmittedEvent", e =>
        {
            var evt = (DomainEvents.RequestSubmittedEvent)e;
            return new RequestSubmittedEventSchema(
                evt.RequestId.Value,
                evt.Descriptor.PrincipalReference,
                evt.Descriptor.RequestType,
                evt.Descriptor.RequestScope,
                evt.SubmittedAt.Value);
        });
        sink.RegisterPayloadMapper("RequestApprovedEvent", e =>
        {
            var evt = (DomainEvents.RequestApprovedEvent)e;
            return new RequestApprovedEventSchema(evt.RequestId.Value);
        });
        sink.RegisterPayloadMapper("RequestDeniedEvent", e =>
        {
            var evt = (DomainEvents.RequestDeniedEvent)e;
            return new RequestDeniedEventSchema(evt.RequestId.Value, evt.Reason);
        });
        sink.RegisterPayloadMapper("RequestWithdrawnEvent", e =>
        {
            var evt = (DomainEvents.RequestWithdrawnEvent)e;
            return new RequestWithdrawnEventSchema(evt.RequestId.Value);
        });
    }
}
