namespace Whycespace.Engines.T2E.Business.Resource.Facility;

public record FacilityCommand(
    string Action,
    string EntityId,
    object Payload
);
