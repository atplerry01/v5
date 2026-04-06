using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T0U.CoreSystem.SystemState;

/// <summary>
/// T0U system state engine. Stateless utility for validating
/// system state authority. NO domain imports, NO persistence.
/// Returns system state validation results only.
/// </summary>
public sealed class SystemStateEngine : IEngine
{
    public string EngineId => "coresystem.system-state.v1";

    private readonly SystemStateValidationGate _gate;

    public SystemStateEngine(SystemStateValidationGate gate)
    {
        ArgumentNullException.ThrowIfNull(gate);
        _gate = gate;
    }

    public async Task<EngineResult> ExecuteAsync(EngineRequest request, CancellationToken cancellationToken = default)
    {
        var entityId = request.Headers.TryGetValue("x-entity-id", out var id) ? id : string.Empty;
        var validation = await _gate.ValidateAsync(request.CommandType, entityId, cancellationToken);

        return validation.IsValid
            ? EngineResult.Ok(new { SystemStateValid = true, EntityId = entityId })
            : EngineResult.Fail(validation.Reason ?? "System state validation failed.", "SYSTEM_STATE_VIOLATION");
    }
}
