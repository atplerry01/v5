namespace Whyce.Shared.Contracts.Runtime;

public sealed record WorkflowStartCommand(Guid Id, string WorkflowName, object Payload);
