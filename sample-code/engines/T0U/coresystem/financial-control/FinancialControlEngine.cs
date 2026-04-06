using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T0U.CoreSystem.FinancialControl;

/// <summary>
/// T0U financial control engine. Stateless utility for validating
/// financial invariant compliance. NO domain imports, NO persistence.
/// Returns validation results only.
/// </summary>
public sealed class FinancialControlEngine : IEngine
{
    public string EngineId => "coresystem.financial-control.v1";

    private readonly FinancialControlValidationGate _gate;

    public FinancialControlEngine(FinancialControlValidationGate gate)
    {
        ArgumentNullException.ThrowIfNull(gate);
        _gate = gate;
    }

    public async Task<EngineResult> ExecuteAsync(EngineRequest request, CancellationToken cancellationToken = default)
    {
        var entityId = request.Headers.TryGetValue("x-entity-id", out var id) ? id : string.Empty;
        var validation = await _gate.ValidateAsync(request.CommandType, entityId, cancellationToken);

        return validation.IsValid
            ? EngineResult.Ok(new { Validated = true, EntityId = entityId })
            : EngineResult.Fail(validation.Reason ?? "Financial control validation failed.", "FINANCIAL_CONTROL_VIOLATION");
    }
}
