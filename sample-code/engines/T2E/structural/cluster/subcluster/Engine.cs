using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Structural;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Structural.Cluster.Subcluster;

public sealed class SubclusterEngine : IEngine<SubclusterCommand>
{
    private readonly SubclusterPolicyAdapter _policy = new();
    private readonly IClusterDomainService _clusterDomainService;

    public SubclusterEngine(IClusterDomainService clusterDomainService)
    {
        _clusterDomainService = clusterDomainService;
    }

    public async Task<EngineResult> ExecuteAsync(SubclusterCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            CreateSubClusterCommand c => await CreateAsync(c, context, ct),
            ActivateSubClusterCommand c => await ActivateAsync(c, context, ct),
            DeactivateSubClusterCommand c => await DeactivateAsync(c, context, ct),
            AddSubClusterSpvCommand c => await AddSpvAsync(c, context, ct),
            RemoveSubClusterSpvCommand c => await RemoveSpvAsync(c, context, ct),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> CreateAsync(CreateSubClusterCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = BuildContext(context, command);
        var result = await _clusterDomainService.CreateSubClusterAsync(execCtx, command.Id, command.AuthorityId, command.Name);
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }

    private async Task<EngineResult> ActivateAsync(ActivateSubClusterCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = BuildContext(context, command);
        var result = await _clusterDomainService.ActivateSubClusterAsync(execCtx, command.Id);
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }

    private async Task<EngineResult> DeactivateAsync(DeactivateSubClusterCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = BuildContext(context, command);
        var result = await _clusterDomainService.DeactivateSubClusterAsync(execCtx, command.Id, command.Reason);
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }

    private async Task<EngineResult> AddSpvAsync(AddSubClusterSpvCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = BuildContext(context, command);
        var result = await _clusterDomainService.AddSubClusterSpvAsync(execCtx, command.SubClusterId, command.SpvId);
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }

    private async Task<EngineResult> RemoveSpvAsync(RemoveSubClusterSpvCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = BuildContext(context, command);
        var result = await _clusterDomainService.RemoveSubClusterSpvAsync(execCtx, command.SubClusterId, command.SpvId);
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }

    private static DomainExecutionContext BuildContext(EngineContext context, SubclusterCommand command) => new()
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
