using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Structural;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Structural.Cluster.Classification;

public sealed class ClassificationEngine
{
    private readonly ClassificationPolicyAdapter _policy = new();
    private readonly IClusterDomainService _clusterDomainService;

    public ClassificationEngine(IClusterDomainService clusterDomainService)
    {
        _clusterDomainService = clusterDomainService;
    }

    public async Task<EngineResult> ExecuteAsync(ClassificationCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);
        return command switch
        {
            CreateClassificationCommand c => await CreateAsync(c, context, ct),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> CreateAsync(CreateClassificationCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = new DomainExecutionContext
        {
            CorrelationId = context.CorrelationId,
            ActorId = context.Headers.GetValueOrDefault("x-actor-id") ?? "system",
            Action = command.GetType().Name,
            Domain = "structural.cluster",
            CommandType = context.CommandType,
            PolicyId = context.Headers.GetValueOrDefault("x-policy-id"),
            Timestamp = DateTimeOffset.UtcNow
        };

        var result = await _clusterDomainService.CreateClassificationAsync(execCtx, command.Id);
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }
}
