using Whycespace.Runtime.Command;

namespace Whycespace.Runtime.Dispatcher;

public sealed class WorkflowResolver
{
    private readonly Dictionary<string, Func<CommandContext, Task<WorkflowStep[]>>> _workflows = new();

    public void Register(string commandType, Func<CommandContext, Task<WorkflowStep[]>> resolver)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandType);
        ArgumentNullException.ThrowIfNull(resolver);
        _workflows[commandType] = resolver;
    }

    public async Task<WorkflowStep[]> ResolveAsync(CommandContext context)
    {
        if (_workflows.TryGetValue(context.Envelope.CommandType, out var resolver))
        {
            return await resolver(context);
        }

        // Default: single-step workflow targeting the command type's engine directly
        return [new WorkflowStep { EngineCommandType = context.Envelope.CommandType }];
    }
}

public sealed record WorkflowStep
{
    public required string EngineCommandType { get; init; }
    public object? TransformedPayload { get; init; }
}
