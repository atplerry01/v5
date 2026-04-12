namespace Whycespace.Domain.BusinessSystem.Integration.EventBridge;

public sealed record EventBridgeCreatedEvent(EventBridgeId EventBridgeId, EventMappingId MappingId);
