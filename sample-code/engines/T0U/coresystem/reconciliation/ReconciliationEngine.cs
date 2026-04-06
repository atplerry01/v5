using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T0U.CoreSystem.Reconciliation;

/// <summary>
/// T0U reconciliation engine. Stateless utility for validating
/// system consistency post-execution. NO domain imports, NO persistence.
/// Returns consistency verification results only.
/// </summary>
public sealed class ReconciliationEngine : IEngine
{
    public string EngineId => "coresystem.reconciliation.v1";

    private readonly ReconciliationValidationGate _gate;

    public ReconciliationEngine(ReconciliationValidationGate gate)
    {
        ArgumentNullException.ThrowIfNull(gate);
        _gate = gate;
    }

    public async Task<EngineResult> ExecuteAsync(EngineRequest request, CancellationToken cancellationToken = default)
    {
        var entityId = request.Headers.TryGetValue("x-entity-id", out var id) ? id : string.Empty;
        var validation = await _gate.ValidateAsync(request.CommandType, entityId, cancellationToken);

        return validation.IsValid
            ? EngineResult.Ok(new { Verified = true, EntityId = entityId })
            : EngineResult.Fail(validation.Reason ?? "Reconciliation validation failed.", "RECONCILIATION_VIOLATION");
    }
}
