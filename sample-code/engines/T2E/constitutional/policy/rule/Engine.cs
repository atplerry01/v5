using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Constitutional;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Constitutional.Policy.Rule;

public sealed class PolicyRuleEngine
{
    private readonly PolicyRulePolicyAdapter _policy = new();
    private readonly IPolicyDomainService _policyDomainService;

    public PolicyRuleEngine(IPolicyDomainService policyDomainService)
    {
        _policyDomainService = policyDomainService;
    }

    public async Task<EngineResult> ExecuteAsync(PolicyRuleCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            CreatePolicyRuleCommand c => await CreateAsync(c, context, ct),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> CreateAsync(CreatePolicyRuleCommand command, EngineContext context, CancellationToken ct)
    {
        var validation = await context.ValidateAsync(command.Id, ct);
        if (!validation.IsValid)
            return EngineResult.Fail(validation.Reason ?? "Validation failed", "POLICY_RULE_CREATE_INVALID");

        var execCtx = new DomainExecutionContext
        {
            CorrelationId = context.CorrelationId,
            ActorId = context.Headers.GetValueOrDefault("x-actor-id") ?? "system",
            Action = command.GetType().Name,
            Domain = "constitutional.policy",
            CommandType = context.CommandType,
            PolicyId = context.Headers.GetValueOrDefault("x-policy-id"),
            Timestamp = DateTimeOffset.UtcNow
        };

        var result = await _policyDomainService.CreateRuleAsync(execCtx, command.Id);
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }
}
