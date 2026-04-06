using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Structural;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Structural.Cluster.Authority;

public sealed class AuthorityEngine
{
    private readonly AuthorityPolicyAdapter _policy = new();
    private readonly IClusterDomainService _clusterDomainService;

    public AuthorityEngine(IClusterDomainService clusterDomainService)
    {
        _clusterDomainService = clusterDomainService;
    }

    public async Task<EngineResult> ExecuteAsync(AuthorityCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            CreateAuthorityCommand c => await CreateAsync(c, context, ct),
            AddAuthoritySubClusterCommand c => await AddSubClusterAsync(c, context, ct),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> CreateAsync(CreateAuthorityCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = BuildContext(context, command);
        var result = await _clusterDomainService.CreateAuthorityAsync(execCtx, command.Id, command.ClusterId, command.Name);
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }

    private async Task<EngineResult> AddSubClusterAsync(AddAuthoritySubClusterCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = BuildContext(context, command);
        var result = await _clusterDomainService.AddAuthoritySubClusterAsync(execCtx, command.AuthorityId, command.SubClusterId);
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }

    private static DomainExecutionContext BuildContext(EngineContext context, AuthorityCommand command) => new()
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
