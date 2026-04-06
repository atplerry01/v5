using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Structural;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Structural.Cluster.Topology;

public sealed class TopologyEngine
{
    private readonly TopologyPolicyAdapter _policy = new();
    private readonly IClusterDomainService _clusterDomainService;

    public TopologyEngine(IClusterDomainService clusterDomainService)
    {
        _clusterDomainService = clusterDomainService;
    }

    public async Task<EngineResult> ExecuteAsync(TopologyCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);
        return command switch
        {
            CreateTopologyCommand c => await CreateAsync(c, context, ct),
            AddTopologySpvCommand c => await AddSpvAsync(c, context, ct),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> CreateAsync(CreateTopologyCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = BuildContext(context, command);
        var result = await _clusterDomainService.CreateTopologyAsync(execCtx, command.Id, command.AuthorityId, command.Name);
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }

    private async Task<EngineResult> AddSpvAsync(AddTopologySpvCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = BuildContext(context, command);
        var result = await _clusterDomainService.AddTopologySpvAsync(execCtx, command.SubClusterId, command.SpvId);
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }

    private static DomainExecutionContext BuildContext(EngineContext context, TopologyCommand command) => new()
    {
        CorrelationId = context.CorrelationId,
        ActorId = context.Headers.GetValueOrDefault("x-actor-id") ?? "system",
        Action = command.GetType().Name,
        Domain = "structural.cluster",
        CommandType = context.CommandType,
        PolicyId = context.Headers.GetValueOrDefault("x-policy-id"),
            Timestamp = DateTimeOffset.UtcNow
    };
}
