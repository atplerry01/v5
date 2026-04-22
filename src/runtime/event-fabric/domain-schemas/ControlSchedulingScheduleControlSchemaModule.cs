using Whycespace.Shared.Contracts.Events.Control.Scheduling.ScheduleControl;
using DomainEvents = Whycespace.Domain.ControlSystem.Scheduling.ScheduleControl;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlSchedulingScheduleControlSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("ScheduleControlDefinedEvent", EventVersion.Default,
            typeof(DomainEvents.ScheduleControlDefinedEvent), typeof(ScheduleControlDefinedEventSchema));
        sink.RegisterSchema("ScheduleControlSuspendedEvent", EventVersion.Default,
            typeof(DomainEvents.ScheduleControlSuspendedEvent), typeof(ScheduleControlSuspendedEventSchema));
        sink.RegisterSchema("ScheduleControlResumedEvent", EventVersion.Default,
            typeof(DomainEvents.ScheduleControlResumedEvent), typeof(ScheduleControlResumedEventSchema));
        sink.RegisterSchema("ScheduleControlRetiredEvent", EventVersion.Default,
            typeof(DomainEvents.ScheduleControlRetiredEvent), typeof(ScheduleControlRetiredEventSchema));

        sink.RegisterPayloadMapper("ScheduleControlDefinedEvent", e =>
        {
            var evt = (DomainEvents.ScheduleControlDefinedEvent)e;
            return new ScheduleControlDefinedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.JobDefinitionId,
                evt.TriggerExpression);
        });
        sink.RegisterPayloadMapper("ScheduleControlSuspendedEvent", e =>
        {
            var evt = (DomainEvents.ScheduleControlSuspendedEvent)e;
            return new ScheduleControlSuspendedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"));
        });
        sink.RegisterPayloadMapper("ScheduleControlResumedEvent", e =>
        {
            var evt = (DomainEvents.ScheduleControlResumedEvent)e;
            return new ScheduleControlResumedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"));
        });
        sink.RegisterPayloadMapper("ScheduleControlRetiredEvent", e =>
        {
            var evt = (DomainEvents.ScheduleControlRetiredEvent)e;
            return new ScheduleControlRetiredEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"));
        });
    }
}
