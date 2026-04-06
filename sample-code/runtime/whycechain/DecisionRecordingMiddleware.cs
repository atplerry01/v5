using Whycespace.Runtime.Command;
using Whycespace.Runtime.ControlPlane.Middleware;

namespace Whycespace.Runtime.WhyceChain;

/// <summary>
/// Observability middleware that captures all decisions made during execution
/// and records them to WhyceChain via IDecisionRecorder.
///
/// This middleware runs as the outermost observability layer — it wraps the
/// entire pipeline and collects decisions accumulated by inner middleware
/// (authorization, policy, execution guard) via CommandContext properties.
/// </summary>
public sealed class DecisionRecordingMiddleware : IMiddleware
{
    private readonly IDecisionRecorder _recorder;

    public DecisionRecordingMiddleware(IDecisionRecorder recorder)
    {
        ArgumentNullException.ThrowIfNull(recorder);
        _recorder = recorder;
    }

    public async Task<CommandResult> InvokeAsync(CommandContext context, MiddlewareDelegate next)
    {
        // Initialize decision collection on context
        var decisions = new List<DecisionEnvelope>();
        context.Set(ContextKeys.Decisions, decisions);

        var result = await next(context);

        // Record all accumulated decisions to WhyceChain
        if (decisions.Count > 0)
        {
            await _recorder.RecordAsync(decisions, context.CancellationToken);
        }

        return result;
    }

    /// <summary>
    /// Helper to append a decision from any middleware that has access to the context.
    /// </summary>
    public static void RecordDecision(CommandContext context, DecisionEnvelope decision)
    {
        var decisions = context.Get<List<DecisionEnvelope>>(ContextKeys.Decisions);
        decisions?.Add(decision);
    }

    public static class ContextKeys
    {
        public const string Decisions = "WhyceChain.Decisions";
    }
}
