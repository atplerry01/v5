using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Economic.Ledger;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Ledger.Settlement;

public sealed class SettlementEngine : IEngine<SettlementCommand>
{
    private readonly SettlementPolicyAdapter _policy = new();
    private readonly ILedgerDomainService _ledgerDomainService;

    public SettlementEngine(ILedgerDomainService ledgerDomainService)
    {
        _ledgerDomainService = ledgerDomainService;
    }

    public async Task<EngineResult> ExecuteAsync(SettlementCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            CreateSettlementCommand c => await CreateAsync(c, context, ct),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> CreateAsync(CreateSettlementCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = new DomainExecutionContext
        {
            CorrelationId = context.CorrelationId,
            ActorId = context.Headers.GetValueOrDefault("x-actor-id") ?? "system",
            Action = command.GetType().Name,
            Domain = "economic.ledger",
            CommandType = context.CommandType,
            PolicyId = context.Headers.GetValueOrDefault("x-policy-id"),
            Timestamp = DateTimeOffset.UtcNow
        };

        var result = await _ledgerDomainService.CreateSettlementAsync(execCtx, command.Id, command.PayeeIdentityId, command.Amount);
        return result.Success ? EngineResult.Ok(result.Data) : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }
}
