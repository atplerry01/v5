using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Economic.Ledger;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Ledger.Obligation;

public sealed class ObligationEngine : IEngine<ObligationCommand>
{
    private readonly ObligationPolicyAdapter _policy = new();
    private readonly IObligationDomainService _obligationService;

    public ObligationEngine(IObligationDomainService obligationService)
    {
        _obligationService = obligationService;
    }

    public async Task<EngineResult> ExecuteAsync(ObligationCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            CreateObligationCommand c => await CreateAsync(c, context, ct),
            ActivateObligationCommand c => await ActivateAsync(c, context, ct),
            SettleObligationCommand c => await SettleAsync(c, context, ct),
            DefaultObligationCommand c => await DefaultAsync(c, context, ct),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> CreateAsync(CreateObligationCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = BuildContext(context, command);
        var result = await _obligationService.CreateAsync(execCtx, command.Id, command.DebtorId, command.CreditorId, command.Amount, command.CurrencyCode);
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }

    private async Task<EngineResult> ActivateAsync(ActivateObligationCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = BuildContext(context, command);
        var result = await _obligationService.ActivateAsync(execCtx, command.ObligationId);
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }

    private async Task<EngineResult> SettleAsync(SettleObligationCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = BuildContext(context, command);
        var result = await _obligationService.SettleAsync(execCtx, command.ObligationId);
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }

    private async Task<EngineResult> DefaultAsync(DefaultObligationCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = BuildContext(context, command);
        var result = await _obligationService.DefaultAsync(execCtx, command.ObligationId, command.Reason);
        return result.Success
            ? EngineResult.Ok(result.Data)
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }

    private static DomainExecutionContext BuildContext(EngineContext context, ObligationCommand command) => new()
    {
        CorrelationId = context.CorrelationId,
        ActorId = context.Headers.GetValueOrDefault("x-actor-id") ?? "system",
        Action = command.GetType().Name,
        Domain = "economic.ledger",
        CommandType = context.CommandType,
        PolicyId = context.Headers.GetValueOrDefault("x-policy-id"),
            Timestamp = DateTimeOffset.UtcNow
    };
}
