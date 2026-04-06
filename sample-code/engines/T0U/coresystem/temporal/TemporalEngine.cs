using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T0U.CoreSystem.Temporal;

/// <summary>
/// T0U temporal engine. Stateless utility for validating
/// temporal ordering guarantees. NO domain imports, NO persistence.
/// Returns temporal validation results only.
/// </summary>
public sealed class TemporalEngine : IEngine
{
    public string EngineId => "coresystem.temporal.v1";

    private readonly TemporalValidationGate _gate;

    public TemporalEngine(TemporalValidationGate gate)
    {
        ArgumentNullException.ThrowIfNull(gate);
        _gate = gate;
    }

    public async Task<EngineResult> ExecuteAsync(EngineRequest request, CancellationToken cancellationToken = default)
    {
        var entityId = request.Headers.TryGetValue("x-entity-id", out var id) ? id : string.Empty;
        var validation = await _gate.ValidateAsync(request.CommandType, entityId, cancellationToken);

        return validation.IsValid
            ? EngineResult.Ok(new { TemporalValid = true, EntityId = entityId })
            : EngineResult.Fail(validation.Reason ?? "Temporal validation failed.", "TEMPORAL_VIOLATION");
    }
}
