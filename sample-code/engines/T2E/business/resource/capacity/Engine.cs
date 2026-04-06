using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Resource;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Resource.Capacity;

public sealed class CapacityEngine
{
    private readonly CapacityPolicyAdapter _policy = new();
    private readonly IResourceDomainService _resourceDomainService;

    public CapacityEngine(IResourceDomainService resourceDomainService)
    {
        _resourceDomainService = resourceDomainService;
    }

    public async Task<EngineResult> ExecuteAsync(CapacityCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);
        return command switch
        {
            CreateCapacityCommand c => await CreateAsync(c, context, ct),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> CreateAsync(CreateCapacityCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = new DomainExecutionContext
        {
            CorrelationId = context.CorrelationId,
            ActorId = context.Headers.GetValueOrDefault("x-actor-id") ?? "system",
            Action = command.GetType().Name,
            Domain = "business.resource",
            CommandType = context.CommandType,
            PolicyId = context.Headers.GetValueOrDefault("x-policy-id"),
            Timestamp = DateTimeOffset.UtcNow
        };

        var result = await _resourceDomainService.CreateCapacityAsync(execCtx, command.Id);
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }
}
