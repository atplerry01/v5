using Whycespace.Runtime.Command;
using Whycespace.Runtime.ControlPlane;
using Whycespace.Runtime.ControlPlane.Policy;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Runtime.Engine;

public sealed class EngineInvoker
{
    private readonly EngineResolver _resolver;

    public EngineInvoker(EngineResolver resolver)
    {
        _resolver = resolver;
    }

    public async Task<CommandResult> InvokeAsync(string commandType, object payload, CommandContext context)
    {
        // Guard 1: Runtime origin
        if (!context.IsFromRuntime)
        {
            throw new RuntimeControlPlaneException(
                "Engine invocation outside the runtime control plane is forbidden. "
                + "All execution must flow through RuntimeControlPlane.ExecuteAsync().",
                "ENGINE_INVOCATION_OUTSIDE_RUNTIME");
        }

        // Guard 2: POLICY ENFORCEMENT LOCK — no command executes without explicit policy decision
        var decision = context.Properties.TryGetValue(PolicyDecision.ContextKey, out var d)
            ? d as PolicyDecision
            : null;

        if (decision is null)
        {
            throw new RuntimeControlPlaneException(
                $"POLICY BYPASS DETECTED: Command '{commandType}' reached EngineInvoker without a PolicyDecision. "
                + "All commands must be evaluated by PolicyMiddleware before engine invocation.",
                "POLICY_BYPASS_DETECTED");
        }

        if (decision.Result == PolicyDecisionResult.Deny)
        {
            throw new RuntimeControlPlaneException(
                $"POLICY DENIED: Command '{commandType}' was denied by policy. Reason: {decision.DenialReason}",
                "POLICY_DENIED_AT_ENGINE");
        }

        // Guard 3: Engine exists
        var descriptor = _resolver.ResolveDescriptor(commandType)
            ?? throw new RuntimeControlPlaneException(
                $"No engine registered for command type '{commandType}'.",
                "ENGINE_NOT_FOUND");

        // Guard 4: Engine is a TypedEngineAdapter
        if (descriptor.Engine is not ITypedEngineAdapter adapter)
        {
            throw new RuntimeControlPlaneException(
                $"Engine '{commandType}' is not a TypedEngineAdapter. Direct engine execution is forbidden.",
                "ENGINE_NOT_ADAPTER");
        }

        // Guard 5: Command type consistency — descriptor must agree with adapter
        if (adapter.CommandType != descriptor.CommandType)
        {
            throw new RuntimeControlPlaneException(
                $"Command type mismatch for engine '{commandType}'. "
                + $"Descriptor declares '{descriptor.CommandType.Name}' but adapter handles '{adapter.CommandType.Name}'.",
                "COMMAND_TYPE_MISMATCH");
        }

        var request = new EngineRequest
        {
            CommandType = commandType,
            Payload = payload,
            CorrelationId = context.Envelope.CorrelationId,
            Headers = context.Envelope.Metadata.Headers
        };

        var result = await descriptor.Engine.ExecuteAsync(request, context.CancellationToken);

        var now = context.Clock.UtcNowOffset;

        return result.Success
            ? CommandResult.Ok(context.Envelope.CommandId, result.Data, now)
            : CommandResult.Fail(context.Envelope.CommandId, result.ErrorMessage ?? "Engine execution failed.", result.ErrorCode, now);
    }
}
