using Whycespace.Runtime.Command;
using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Runtime.ControlPlane.Policy;

/// <summary>
/// Deterministic mapper: CommandContext → PolicyEvaluationInput.
/// Reusable across middleware, simulation, and any policy evaluation entry point.
/// </summary>
public static class PolicyContextMapper
{
    public static PolicyEvaluationInput Map(CommandContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var envelope = context.Envelope;

        var actorId = Guid.TryParse(envelope.Metadata.WhyceId, out var parsed)
            ? parsed
            : Guid.Empty;

        var policyId = Guid.TryParse(envelope.Metadata.PolicyId, out var pid)
            ? (Guid?)pid
            : null;

        var classification = ExtractClassification(envelope.CommandType);
        var environment = ResolveEnvironment(envelope.Metadata.Headers);

        return new PolicyEvaluationInput(
            policyId,
            actorId,
            envelope.CommandType,
            classification,
            environment,
            envelope.Timestamp);
    }

    private static string ExtractClassification(string commandType)
    {
        var dotIndex = commandType.IndexOf('.', StringComparison.Ordinal);
        return dotIndex > 0 ? commandType[..dotIndex] : commandType;
    }

    private static string ResolveEnvironment(IReadOnlyDictionary<string, string> headers)
    {
        return headers.TryGetValue("X-Environment", out var env) ? env : "production";
    }
}
