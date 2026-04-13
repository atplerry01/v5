namespace Whycespace.Shared.Contracts.Runtime;

public sealed record WorkflowStartCommand(Guid Id, string WorkflowName, object Payload);
