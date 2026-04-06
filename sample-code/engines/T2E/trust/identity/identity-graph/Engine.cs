using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Engines.T2E.Trust.Identity.IdentityGraph;

public sealed class IdentityGraphEngine
{
    private readonly IdentityGraphPolicyAdapter _policy = new();

    public async Task<EngineResult> ExecuteAsync(IdentityGraphCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            CreateIdentityGraphCommand c => CreateAsync(c),
            LinkIdentitiesCommand c => LinkAsync(c),
            UnlinkIdentitiesCommand c => UnlinkAsync(c),
            MergeGraphsCommand c => MergeAsync(c),
            CloseGraphCommand c => CloseAsync(c),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private static EngineResult CreateAsync(CreateIdentityGraphCommand c) => EngineResult.Ok(new IdentityGraphDto(DeterministicIdHelper.FromSeed($"IdentityGraph:{c.PrimaryIdentityId}").ToString(), c.PrimaryIdentityId, "Active", 0));
    private static EngineResult LinkAsync(LinkIdentitiesCommand c) => EngineResult.Ok(new { c.GraphId, c.SourceIdentityId, c.TargetIdentityId, c.LinkType, Action = "Linked" });
    private static EngineResult UnlinkAsync(UnlinkIdentitiesCommand c) => EngineResult.Ok(new { c.GraphId, c.SourceIdentityId, c.TargetIdentityId, Action = "Unlinked" });
    private static EngineResult MergeAsync(MergeGraphsCommand c) => EngineResult.Ok(new { c.TargetGraphId, c.SourceGraphId, Action = "Merged" });
    private static EngineResult CloseAsync(CloseGraphCommand c) => EngineResult.Ok(new { c.GraphId, Status = "Closed" });
}
