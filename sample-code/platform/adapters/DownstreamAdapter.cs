using Whycespace.Platform.Middleware;
using Whycespace.Shared.Contracts.Systems;
using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Platform.Adapters;

/// <summary>
/// THIN PASS-THROUGH adapter. Resolves downstream handler and forwards call.
/// ZERO business logic. ZERO branching. ZERO orchestration.
/// Enforces: Platform → Downstream → WSS → Runtime.
/// </summary>
public sealed class DownstreamAdapter
{
    private readonly IProcessHandlerRegistry _registry;
    private readonly IIdGenerator _idGenerator;

    public DownstreamAdapter(IProcessHandlerRegistry registry, IIdGenerator idGenerator)
    {
        _registry = registry;
        _idGenerator = idGenerator;
    }

    public async Task<ApiResponse> SendCommandAsync(
        string commandType,
        object payload,
        string correlationId,
        string? whyceId = null,
        string? policyId = null,
        string? traceId = null,
        string? aggregateId = null,
        CancellationToken cancellationToken = default)
    {
        var handler = _registry.Resolve(commandType);

        var result = await handler.HandleAsync(new ProcessCommand
        {
            CommandId = _idGenerator.DeterministicGuid($"downstream:command:{commandType}:{correlationId}"),
            CommandType = commandType,
            Payload = payload,
            CorrelationId = correlationId,
            AggregateId = aggregateId,
            WhyceId = whyceId,
            PolicyId = policyId
        }, cancellationToken);

        return result.Success
            ? ApiResponse.Ok(result.Data, traceId)
            : ApiResponse.BadRequest(result.ErrorMessage ?? "Command failed", traceId);
    }
}
