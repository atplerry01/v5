namespace Whycespace.Engines.T2E.Business.Integration.Callback;

public record CallbackCommand(
    string Action,
    string EntityId,
    object Payload
);
