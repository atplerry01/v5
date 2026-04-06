using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Decision.Governance.Quorum;

public sealed class QuorumEngine
{
    private readonly QuorumPolicyAdapter _policy = new();

    public async Task<EngineResult> ExecuteAsync(QuorumCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            CreateQuorumCommand c => await CreateAsync(c, context, ct),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> CreateAsync(CreateQuorumCommand command, EngineContext context, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        return await Task.FromResult(EngineResult.Ok(new { command.Id }));
    }
}
