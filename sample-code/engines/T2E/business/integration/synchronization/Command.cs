namespace Whycespace.Engines.T2E.Business.Integration.Synchronization;

public record SynchronizationCommand(
    string Action,
    string EntityId,
    object Payload
);
