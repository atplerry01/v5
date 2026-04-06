using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Economic.Transaction;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Transaction.Limit;

public sealed class LimitEngine
{
    private readonly LimitPolicyAdapter _policy = new();
    private readonly ILimitDomainService _limitDomainService;

    public LimitEngine(ILimitDomainService limitDomainService)
    {
        _limitDomainService = limitDomainService;
    }

    public async Task<EngineResult> ExecuteAsync(LimitCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            EvaluateLimitCommand c => await EvaluateAsync(c, context, ct),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> EvaluateAsync(EvaluateLimitCommand command, EngineContext context, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var execCtx = new DomainExecutionContext
        {
            CorrelationId = context.CorrelationId,
            ActorId = context.Headers.GetValueOrDefault("x-actor-id") ?? "system",
            Action = command.GetType().Name,
            Domain = "economic.transaction",
            CommandType = context.CommandType,
            PolicyId = context.Headers.GetValueOrDefault("x-policy-id"),
            Timestamp = DateTimeOffset.UtcNow
        };

        var result = await _limitDomainService.CreateAndEvaluateAsync(
            execCtx,
            command.LimitId, command.IdentityId,
            command.MaxTransactionLimit, command.DailyLimit, command.MonthlyLimit,
            command.TransactionAmount, command.DailyTotal, command.MonthlyTotal);

        if (!result.Success)
            return EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);

        var evalData = result.Data as LimitEvaluationData;
        if (evalData is null)
            return EngineResult.Fail("Unexpected domain service response", "DOMAIN_ERROR");

        if (!evalData.HasViolations)
            return EngineResult.Ok(new LimitEvaluationDto(
                command.LimitId, command.IdentityId, LimitDecision.Allow, null, null));

        var firstViolation = evalData.Violations[0];
        var decision = firstViolation.LimitType == "MaxTransaction"
            ? LimitDecision.Deny
            : LimitDecision.Conditional;

        return EngineResult.Ok(new LimitEvaluationDto(
            command.LimitId, command.IdentityId, decision,
            $"LIMIT_{firstViolation.LimitType.ToUpperInvariant()}_EXCEEDED",
            firstViolation.LimitType));
    }
}
