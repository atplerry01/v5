namespace Whycespace.Domain.BusinessSystem.Integration.CommandBridge;

public sealed record CommandBridgeCreatedEvent(CommandBridgeId CommandBridgeId, CommandMappingId MappingId);
