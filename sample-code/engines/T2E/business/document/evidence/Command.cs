namespace Whycespace.Engines.T2E.Business.Document.Evidence;

public record EvidenceCommand(string Action, string EntityId, object Payload);
public sealed record CreateEvidenceCommand(string Id) : EvidenceCommand("Create", Id, null!);
