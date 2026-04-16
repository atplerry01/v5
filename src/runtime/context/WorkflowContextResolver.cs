using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Runtime.Context;

/// <summary>
/// Resolves workflow context from the command.
/// Extracts workflow metadata for workflow-type commands
/// (WorkflowStartCommand, WorkflowResumeCommand).
/// </summary>
public sealed class WorkflowContextResolver
{
    /// <summary>
    /// Attempts to resolve workflow context from the command.
    /// Returns null if the command is not a workflow command.
    /// </summary>
    public WorkflowContext? Resolve(object command, CommandContext context)
    {
        if (command is WorkflowStartCommand startCommand)
        {
            return new WorkflowContext
            {
                WorkflowId = startCommand.Id,
                WorkflowName = startCommand.WorkflowName,
                CorrelationId = context.CorrelationId,
                IsResume = false
            };
        }

        if (command is WorkflowResumeCommand resumeCommand)
        {
            return new WorkflowContext
            {
                WorkflowId = resumeCommand.WorkflowId,
                WorkflowName = string.Empty,
                CorrelationId = context.CorrelationId,
                IsResume = true
            };
        }

        return null;
    }
}

public sealed record WorkflowContext
{
    public required Guid WorkflowId { get; init; }
    public required string WorkflowName { get; init; }
    public required Guid CorrelationId { get; init; }
    public required bool IsResume { get; init; }
}
