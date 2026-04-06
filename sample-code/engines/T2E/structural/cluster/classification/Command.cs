namespace Whycespace.Engines.T2E.Structural.Cluster.Classification;

public record ClassificationCommand(string Action, string EntityId, object Payload);
public sealed record CreateClassificationCommand(string Id) : ClassificationCommand("Create", Id, null!);
