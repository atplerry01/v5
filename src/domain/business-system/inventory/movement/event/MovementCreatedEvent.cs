namespace Whycespace.Domain.BusinessSystem.Inventory.Movement;

public sealed record MovementCreatedEvent(MovementId MovementId, MovementSourceId SourceId, MovementTargetId TargetId, int Quantity);
