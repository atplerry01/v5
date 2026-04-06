namespace Whycespace.Engines.T2E.Structural.Cluster.Lifecycle;

public record LifecycleCommand(string Action, string EntityId, object Payload);
