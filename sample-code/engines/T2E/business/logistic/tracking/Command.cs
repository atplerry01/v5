namespace Whycespace.Engines.T2E.Business.Logistic.Tracking;

public record TrackingCommand(
    string Action,
    string EntityId,
    object Payload
);
