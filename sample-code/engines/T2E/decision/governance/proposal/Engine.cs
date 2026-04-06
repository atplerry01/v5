using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Decision.Governance.Proposal;

public sealed class ProposalEngine
{
    private readonly ProposalPolicyAdapter _policy = new();

    public async Task<EngineResult> ExecuteAsync(ProposalCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            CreateProposalCommand c => await CreateAsync(c, context, ct),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> CreateAsync(CreateProposalCommand command, EngineContext context, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        return await Task.FromResult(EngineResult.Ok(new { command.Id }));
    }
}
