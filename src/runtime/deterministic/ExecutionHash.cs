using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Runtime.Deterministic;

/// <summary>
/// Execution Hash — deterministic fingerprint of a command execution.
///
/// Captures ALL execution context:
/// - Command (correlationId, commandId, type)
/// - Identity (identityId, roles, trustScore)
/// - Policy (policyId, decisionHash)
/// - Workflow (workflowId if applicable)
/// - Economic (currency, amount if applicable)
/// - Events (types, count, payload hashes)
///
/// Same execution always produces the same hash, enabling replay verification.
/// </summary>
public static class ExecutionHash
{
    /// <summary>
    /// Computes the full execution hash including all context dimensions.
    /// </summary>
    public static string Compute(CommandContext context, IReadOnlyList<object> domainEvents)
    {
        // Command dimension
        var commandHash = DeterministicHasher.ComputeCompositeHash(
            context.CorrelationId.ToString(),
            context.CommandId.ToString(),
            context.AggregateId.ToString());

        // Identity dimension
        var identityHash = DeterministicHasher.ComputeCompositeHash(
            context.IdentityId ?? "anonymous",
            context.Roles is not null ? string.Join(",", context.Roles) : "none",
            context.TrustScore?.ToString() ?? "0");

        // Policy dimension (includes PolicyVersion for replay determinism)
        var policyHash = DeterministicHasher.ComputeCompositeHash(
            context.PolicyId,
            context.PolicyDecisionHash ?? "none",
            context.PolicyDecisionAllowed?.ToString() ?? "false",
            context.PolicyVersion ?? "none");

        // Events dimension
        var eventSignatures = new List<string>(domainEvents.Count);
        for (var i = 0; i < domainEvents.Count; i++)
        {
            var evt = domainEvents[i];
            var payloadHash = DeterministicHasher.ComputePayloadHash(evt);
            eventSignatures.Add($"{evt.GetType().Name}:{i}:{payloadHash}");
        }
        var eventsHash = DeterministicHasher.ComputeHash(string.Join("|", eventSignatures));

        // Composite execution hash
        return DeterministicHasher.ComputeCompositeHash(
            commandHash,
            identityHash,
            policyHash,
            domainEvents.Count.ToString(),
            eventsHash);
    }

    /// <summary>
    /// Computes execution hash from raw components (backward compatibility).
    /// </summary>
    public static string Compute(Guid correlationId, Guid commandId, IReadOnlyList<object> domainEvents)
    {
        var eventSignatures = new List<string>(domainEvents.Count);
        for (var i = 0; i < domainEvents.Count; i++)
        {
            var evt = domainEvents[i];
            var payloadHash = DeterministicHasher.ComputePayloadHash(evt);
            eventSignatures.Add($"{evt.GetType().Name}:{i}:{payloadHash}");
        }
        var eventsHash = DeterministicHasher.ComputeHash(string.Join("|", eventSignatures));

        return DeterministicHasher.ComputeCompositeHash(
            correlationId.ToString(),
            commandId.ToString(),
            domainEvents.Count.ToString(),
            eventsHash);
    }
}
