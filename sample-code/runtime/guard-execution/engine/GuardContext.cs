using Whycespace.Runtime.Command;
using Whycespace.Runtime.ControlPlane.Policy;

namespace Whycespace.Runtime.GuardExecution.Engine;

public sealed class GuardContext
{
    public required GuardExecutionMode Mode { get; init; }

    // CI mode: static analysis over codebase
    public string? SolutionRoot { get; init; }
    public IReadOnlyList<string> SourceFiles { get; init; } = [];

    // Runtime mode: command execution context
    public string? CommandName { get; init; }
    public object? Payload { get; init; }
    public PolicyDecision? PolicyDecision { get; init; }
    public string? CorrelationId { get; init; }

    // Bypass protection: set to true after PostPolicy guard phase completes
    public bool IsGuardValidated { get; set; }

    // Shared properties bag
    public Dictionary<string, object> Properties { get; } = new();

    public void Set<T>(string key, T value) where T : notnull =>
        Properties[key] = value;

    public T? Get<T>(string key) where T : class =>
        Properties.TryGetValue(key, out var value) ? value as T : null;

    public static GuardContext ForCi(string solutionRoot, IReadOnlyList<string> sourceFiles) =>
        new()
        {
            Mode = GuardExecutionMode.Ci,
            SolutionRoot = solutionRoot,
            SourceFiles = sourceFiles
        };

    public static GuardContext ForRuntime(CommandContext commandContext)
    {
        var envelope = commandContext.Envelope;
        return new GuardContext
        {
            Mode = GuardExecutionMode.Runtime,
            CommandName = envelope.CommandType,
            Payload = envelope.Payload,
            PolicyDecision = commandContext.Get<PolicyDecision>(PolicyDecision.ContextKey),
            CorrelationId = envelope.CorrelationId
        };
    }
}

public enum GuardExecutionMode
{
    Ci,
    Runtime
}
