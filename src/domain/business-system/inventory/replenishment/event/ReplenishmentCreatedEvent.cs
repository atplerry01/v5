namespace Whycespace.Domain.BusinessSystem.Inventory.Replenishment;

public sealed record ReplenishmentCreatedEvent(
    ReplenishmentId ReplenishmentId,
    ReplenishmentThreshold Threshold,
    RestockQuantity RestockQuantity);
