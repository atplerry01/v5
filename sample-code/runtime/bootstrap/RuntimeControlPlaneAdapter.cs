using Whycespace.Runtime.Command;
using Whycespace.Runtime.ControlPlane;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Runtime.Bootstrap;

/// <summary>
/// Bridges the shared IRuntimeControlPlane contract (used by platform's RuntimeAdapter)
/// to the internal RuntimeControlPlane (middleware pipeline).
///
/// Converts RuntimeCommandEnvelope → CommandEnvelope and CommandResult → RuntimeCommandResult.
/// This is the sole entry point from the platform layer into the runtime.
/// </summary>
public sealed class RuntimeControlPlaneAdapter : IRuntimeControlPlane
{
    private readonly RuntimeControlPlane _controlPlane;

    public RuntimeControlPlaneAdapter(RuntimeControlPlane controlPlane)
    {
        ArgumentNullException.ThrowIfNull(controlPlane);
        _controlPlane = controlPlane;
    }

    public async Task<RuntimeCommandResult> ExecuteAsync(
        RuntimeCommandEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        var internalEnvelope = new CommandEnvelope
        {
            CommandId = envelope.CommandId,
            CommandType = envelope.CommandType,
            Payload = envelope.Payload,
            CorrelationId = envelope.CorrelationId,
            Timestamp = envelope.Timestamp,
            AggregateId = envelope.AggregateId,
            Metadata = new CommandMetadata
            {
                CausationId = envelope.CausationId,
                WhyceId = envelope.WhyceId,
                PolicyId = envelope.PolicyId,
                Headers = envelope.Headers
            }
        };

        var result = await _controlPlane.ExecuteAsync(internalEnvelope, cancellationToken);

        return result.Success
            ? RuntimeCommandResult.Ok(result.CommandId, result.Data)
            : RuntimeCommandResult.Fail(result.CommandId, result.ErrorMessage ?? "Command failed.", result.ErrorCode);
    }
}
