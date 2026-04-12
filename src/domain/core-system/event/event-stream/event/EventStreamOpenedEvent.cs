namespace Whycespace.Domain.CoreSystem.Event.EventStream;

public sealed record EventStreamOpenedEvent(EventStreamId EventStreamId, StreamDescriptor Descriptor);
