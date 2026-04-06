namespace Whycespace.Engines.T2E.Business.Resource.Equipment;

public record EquipmentCommand(
    string Action,
    string EntityId,
    object Payload
);
