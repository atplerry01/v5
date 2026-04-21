using Whycespace.Shared.Contracts.Events.Business.Agreement.ChangeControl.Approval;
using DomainEvents = Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Approval;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class BusinessAgreementChangeControlApprovalSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "ApprovalCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ApprovalCreatedEvent),
            typeof(ApprovalCreatedEventSchema));

        sink.RegisterSchema(
            "ApprovalApprovedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ApprovalApprovedEvent),
            typeof(ApprovalApprovedEventSchema));

        sink.RegisterSchema(
            "ApprovalRejectedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ApprovalRejectedEvent),
            typeof(ApprovalRejectedEventSchema));

        sink.RegisterPayloadMapper("ApprovalCreatedEvent", e =>
        {
            var evt = (DomainEvents.ApprovalCreatedEvent)e;
            return new ApprovalCreatedEventSchema(evt.ApprovalId.Value);
        });
        sink.RegisterPayloadMapper("ApprovalApprovedEvent", e =>
        {
            var evt = (DomainEvents.ApprovalApprovedEvent)e;
            return new ApprovalApprovedEventSchema(evt.ApprovalId.Value);
        });
        sink.RegisterPayloadMapper("ApprovalRejectedEvent", e =>
        {
            var evt = (DomainEvents.ApprovalRejectedEvent)e;
            return new ApprovalRejectedEventSchema(evt.ApprovalId.Value);
        });
    }
}
