using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Resource;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Resource.Reservation;

public sealed class ReservationEngine
{
    private readonly ReservationPolicyAdapter _policy = new();
    private readonly IResourceDomainService _resourceDomainService;

    public ReservationEngine(IResourceDomainService resourceDomainService)
    {
        _resourceDomainService = resourceDomainService;
    }

    public async Task<EngineResult> ExecuteAsync(ReservationCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);
        return command switch
        {
            CreateReservationCommand c => await CreateAsync(c, context, ct),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> CreateAsync(CreateReservationCommand command, EngineContext context, CancellationToken ct)
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

        var result = await _resourceDomainService.CreateReservationAsync(execCtx, command.Id);
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }
}
