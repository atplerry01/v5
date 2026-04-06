namespace Whycespace.Engines.T2E.Business.Logistic.Dispatch;

public record DispatchCommand(
    string Action,
    string EntityId,
    object Payload
);
