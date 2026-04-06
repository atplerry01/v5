namespace Whycespace.Engines.T2E.Business.Integration.Replay;

public record ReplayCommand(
    string Action,
    string EntityId,
    object Payload
);
