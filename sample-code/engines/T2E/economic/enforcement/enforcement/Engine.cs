using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Economic.Enforcement;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Enforcement.Enforcement;

public sealed class EnforcementEngine
{
    private readonly EnforcementPolicyAdapter _policy = new();
    private readonly IEconomicEnforcementDomainService _enforcementDomainService;

    public EnforcementEngine(IEconomicEnforcementDomainService enforcementDomainService)
    {
        _enforcementDomainService = enforcementDomainService;
    }

    public async Task<EngineResult> ExecuteAsync(EnforcementCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            ApplyEnforcementCommand c => await ApplyAsync(c, context, ct),
            ReleaseEnforcementCommand c => await ReleaseAsync(c, context, ct),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> ApplyAsync(ApplyEnforcementCommand command, EngineContext context, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validation = await context.ValidateAsync(command.IdentityId, ct);
        if (!validation.IsValid)
            return EngineResult.Fail(validation.Reason ?? "Validation failed", "VALIDATION_FAILED");

        var execCtx = new DomainExecutionContext
        {
            CorrelationId = context.CorrelationId,
            ActorId = context.Headers.GetValueOrDefault("x-actor-id") ?? "system",
            Action = command.GetType().Name,
            Domain = "economic.enforcement",
            CommandType = context.CommandType,
            PolicyId = context.Headers.GetValueOrDefault("x-policy-id"),
            Timestamp = DateTimeOffset.UtcNow
        };

        var result = await _enforcementDomainService.ApplyAsync(
            execCtx,
            command.EnforcementId, command.IdentityId, command.Reason,
            command.EnforcementType, command.Scope, command.Duration);

        if (!result.Success)
            return EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);

        var data = result.Data as EnforcementApplyData
            ?? throw new InvalidOperationException("Unexpected domain service response");

        var decision = data.Decision switch
        {
            "Deny" => EnforcementDecision.Deny,
            "Conditional" => EnforcementDecision.Conditional,
            _ => EnforcementDecision.Deny
        };

        return EngineResult.Ok(new EnforcementDto(
            command.EnforcementId, command.IdentityId,
            command.EnforcementType, command.Scope, command.Duration, "Active",
            decision, data.ReasonCode));
    }

    private async Task<EngineResult> ReleaseAsync(ReleaseEnforcementCommand command, EngineContext context, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var execCtx = new DomainExecutionContext
        {
            CorrelationId = context.CorrelationId,
            ActorId = context.Headers.GetValueOrDefault("x-actor-id") ?? "system",
            Action = command.GetType().Name,
            Domain = "economic.enforcement",
            CommandType = context.CommandType,
            PolicyId = context.Headers.GetValueOrDefault("x-policy-id"),
            Timestamp = DateTimeOffset.UtcNow
        };

        var result = await _enforcementDomainService.ReleaseAsync(execCtx, command.EnforcementId);
        return result.Success
            ? EngineResult.Ok(new { command.EnforcementId, Status = "Released" })
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }
}
