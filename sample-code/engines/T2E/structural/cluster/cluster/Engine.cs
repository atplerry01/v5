using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Structural;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Structural.Cluster.Cluster;

public sealed class ClusterEngine
{
    private readonly ClusterPolicyAdapter _policy = new();
    private readonly IClusterDomainService _clusterDomainService;

    public ClusterEngine(IClusterDomainService clusterDomainService)
    {
        _clusterDomainService = clusterDomainService;
    }

    public async Task<EngineResult> ExecuteAsync(ClusterCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);
        return command switch
        {
            CreateClusterCommand c => await CreateAsync(c, context, ct),
            ActivateClusterCommand c => await ActivateAsync(c, context, ct),
            AddClusterAuthorityCommand c => await AddAuthorityAsync(c, context, ct),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> CreateAsync(CreateClusterCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = BuildContext(context, command);
        var result = await _clusterDomainService.CreateClusterAsync(execCtx, command.Id, command.Name, command.Jurisdiction);
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }

    private async Task<EngineResult> ActivateAsync(ActivateClusterCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = BuildContext(context, command);
        var result = await _clusterDomainService.ActivateClusterAsync(execCtx, command.Id);
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }

    private async Task<EngineResult> AddAuthorityAsync(AddClusterAuthorityCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = BuildContext(context, command);
        var result = await _clusterDomainService.AddClusterAuthorityAsync(execCtx, command.ClusterId, command.AuthorityId);
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }

    private static DomainExecutionContext BuildContext(EngineContext context, ClusterCommand command) => new()
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
