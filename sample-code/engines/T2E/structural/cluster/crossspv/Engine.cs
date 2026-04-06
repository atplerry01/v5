using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Structural;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Structural.Cluster.CrossSpv;

public sealed class CrossSpvEngine
{
    private readonly CrossSpvPolicyAdapter _policy = new();
    private readonly ICrossSpvDomainService _domainService;

    public CrossSpvEngine(ICrossSpvDomainService domainService)
    {
        _domainService = domainService;
    }

    public async Task<EngineResult> ExecuteAsync(CrossSpvCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);
        return command switch
        {
            CreateCrossSpvTransactionCommand c => await CreateAsync(c, context, ct),
            PrepareCrossSpvTransactionCommand c => await PrepareAsync(c, context, ct),
            CommitCrossSpvTransactionCommand c => await CommitAsync(c, context, ct),
            FailCrossSpvTransactionCommand c => await FailAsync(c, context, ct),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> CreateAsync(CreateCrossSpvTransactionCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = BuildContext(context, command);
        var result = await _domainService.CreateCrossSpvTransactionAsync(
            execCtx, command.Id, command.RootSpvId, command.Legs);
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }

    private async Task<EngineResult> PrepareAsync(PrepareCrossSpvTransactionCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = BuildContext(context, command);
        var result = await _domainService.PrepareCrossSpvTransactionAsync(
            execCtx, command.Id, command.TransactionId);
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }

    private async Task<EngineResult> CommitAsync(CommitCrossSpvTransactionCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = BuildContext(context, command);
        var result = await _domainService.CommitCrossSpvTransactionAsync(
            execCtx, command.Id, command.TransactionId);
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }

    private async Task<EngineResult> FailAsync(FailCrossSpvTransactionCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = BuildContext(context, command);
        var result = await _domainService.FailCrossSpvTransactionAsync(
            execCtx, command.Id, command.TransactionId, command.Reason);
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }

    private static DomainExecutionContext BuildContext(EngineContext context, CrossSpvCommand command) => new()
    {
        CorrelationId = context.CorrelationId,
        ActorId = context.Headers.GetValueOrDefault("x-actor-id") ?? "system",
        Action = command.GetType().Name,
        Domain = "structural.cluster.crossspv",
        CommandType = context.CommandType,
        PolicyId = context.Headers.GetValueOrDefault("x-policy-id"),
            Timestamp = DateTimeOffset.UtcNow
    };
}
