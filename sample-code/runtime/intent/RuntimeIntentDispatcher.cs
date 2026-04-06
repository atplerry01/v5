using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Systems.Intent;

namespace Whycespace.Runtime.Intent;

/// <summary>
/// Runtime-side implementation of ISystemIntentDispatcher.
/// Bridges system intents to the runtime control plane.
/// Registered in DI — systems layer never references this directly.
/// </summary>
public sealed class RuntimeIntentDispatcher : ISystemIntentDispatcher
{
    private readonly IRuntimeControlPlane _controlPlane;

    public RuntimeIntentDispatcher(IRuntimeControlPlane controlPlane)
    {
        _controlPlane = controlPlane;
    }

    public async Task<IntentResult> DispatchAsync(ExecuteCommandIntent intent, CancellationToken cancellationToken = default)
    {
        var envelope = new RuntimeCommandEnvelope
        {
            CommandId = intent.CommandId,
            CommandType = intent.CommandType,
            Payload = intent.Payload,
            CorrelationId = intent.CorrelationId,
            Timestamp = intent.Timestamp,
            AggregateId = intent.AggregateId,
            CausationId = intent.CausationId,
            WhyceId = intent.WhyceId,
            PolicyId = intent.PolicyId,
            Headers = intent.Headers
        };

        var result = await _controlPlane.ExecuteAsync(envelope, cancellationToken);

        return new IntentResult
        {
            CommandId = result.CommandId,
            Success = result.Success,
            Data = result.Data,
            ErrorMessage = result.ErrorMessage,
            ErrorCode = result.ErrorCode
        };
    }
}
