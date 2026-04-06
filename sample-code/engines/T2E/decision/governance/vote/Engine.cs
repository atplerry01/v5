using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Decision.Governance.Vote;

public sealed class VoteEngine
{
    private readonly VotePolicyAdapter _policy = new();

    public async Task<EngineResult> ExecuteAsync(VoteCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            CreateVoteCommand c => await CreateAsync(c, context, ct),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> CreateAsync(CreateVoteCommand command, EngineContext context, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        return await Task.FromResult(EngineResult.Ok(new { command.Id }));
    }
}
