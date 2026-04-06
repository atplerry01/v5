using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Structural;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Structural.Cluster.Spv;

public sealed class SpvEngine
{
    private readonly SpvPolicyAdapter _policy = new();
    private readonly IClusterDomainService _clusterDomainService;

    public SpvEngine(IClusterDomainService clusterDomainService)
    {
        _clusterDomainService = clusterDomainService;
    }

    public async Task<EngineResult> ExecuteAsync(SpvCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);
        return command switch
        {
            CreateSpvCommand c => await CreateAsync(c, context, ct),
            ActivateSpvCommand c => await ActivateAsync(c, context, ct),
            SuspendSpvCommand c => await SuspendAsync(c, context, ct),
            ReactivateSpvCommand c => await ReactivateAsync(c, context, ct),
            TerminateSpvCommand c => await TerminateAsync(c, context, ct),
            CloseSpvCommand c => await CloseAsync(c, context, ct),
            AddSpvOperatorCommand c => await AddOperatorAsync(c, context, ct),
            ReplaceSpvOperatorCommand c => await ReplaceOperatorAsync(c, context, ct),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> CreateAsync(CreateSpvCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = BuildContext(context, command);
        var result = await _clusterDomainService.CreateSpvAsync(execCtx, command.Id, command.SubClusterId, command.Name);
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }

    private async Task<EngineResult> ActivateAsync(ActivateSpvCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = BuildContext(context, command);
        var result = await _clusterDomainService.ActivateSpvAsync(execCtx, command.Id);
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }

    private async Task<EngineResult> SuspendAsync(SuspendSpvCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = BuildContext(context, command);
        var result = await _clusterDomainService.SuspendSpvAsync(execCtx, command.Id, command.Reason);
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }

    private async Task<EngineResult> ReactivateAsync(ReactivateSpvCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = BuildContext(context, command);
        var result = await _clusterDomainService.ReactivateSpvAsync(execCtx, command.Id);
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }

    private async Task<EngineResult> TerminateAsync(TerminateSpvCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = BuildContext(context, command);
        var result = await _clusterDomainService.TerminateSpvAsync(execCtx, command.Id, command.Reason);
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }

    private async Task<EngineResult> CloseAsync(CloseSpvCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = BuildContext(context, command);
        var result = await _clusterDomainService.CloseSpvAsync(execCtx, command.Id, command.AuditRecordId);
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }

    private async Task<EngineResult> AddOperatorAsync(AddSpvOperatorCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = BuildContext(context, command);
        var result = await _clusterDomainService.AddSpvOperatorAsync(execCtx, command.SpvId, command.OperatorId);
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }

    private async Task<EngineResult> ReplaceOperatorAsync(ReplaceSpvOperatorCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = BuildContext(context, command);
        var result = await _clusterDomainService.ReplaceSpvOperatorAsync(execCtx, command.SpvId, command.OldOperatorId, command.NewOperatorId);
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }

    private static DomainExecutionContext BuildContext(EngineContext context, SpvCommand command) => new()
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
