using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Decision.Governance.Delegation;

public sealed class DelegationEngine
{
    private readonly DelegationPolicyAdapter _policy = new();

    public async Task<EngineResult> ExecuteAsync(DelegationCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            CreateDelegationCommand c => await CreateAsync(c, context, ct),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> CreateAsync(CreateDelegationCommand command, EngineContext context, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        return await Task.FromResult(EngineResult.Ok(new { command.Id }));
    }
}
