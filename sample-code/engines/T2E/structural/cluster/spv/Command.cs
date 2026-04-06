namespace Whycespace.Engines.T2E.Structural.Cluster.Spv;

public record SpvCommand(string Action, string EntityId, object Payload);
public sealed record CreateSpvCommand(string Id, string SubClusterId, string Name) : SpvCommand("Create", Id, null!);
public sealed record ActivateSpvCommand(string Id) : SpvCommand("Activate", Id, null!);
public sealed record SuspendSpvCommand(string Id, string Reason) : SpvCommand("Suspend", Id, null!);
public sealed record ReactivateSpvCommand(string Id) : SpvCommand("Reactivate", Id, null!);
public sealed record TerminateSpvCommand(string Id, string Reason) : SpvCommand("Terminate", Id, null!);
public sealed record CloseSpvCommand(string Id, string AuditRecordId) : SpvCommand("Close", Id, null!);
public sealed record AddSpvOperatorCommand(string SpvId, string OperatorId) : SpvCommand("AddOperator", SpvId, null!);
public sealed record ReplaceSpvOperatorCommand(string SpvId, string OldOperatorId, string NewOperatorId) : SpvCommand("ReplaceOperator", SpvId, null!);
