using Whycespace.Domain.EconomicSystem.Capital.Capital;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Capital.Capital;

public sealed class CapitalEngine : IEngine<CapitalCommand>
{
    private readonly CapitalPolicyAdapter _policy = new();

    public async Task<EngineResult> ExecuteAsync(CapitalCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            CreateCapitalCommand c => await CreateAsync(c, context),
            CommitCapitalCommand c => await CommitAsync(c, context),
            AllocateCapitalCommand c => await AllocateAsync(c, context),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private static async Task<EngineResult> CreateAsync(CreateCapitalCommand command, EngineContext context)
    {
        var aggregate = await context.LoadAggregate<CapitalAccountAggregate>(command.Id);
        aggregate.Create(Guid.Parse(command.Id));
        await context.EmitEvents(aggregate);
        return EngineResult.Ok(context.EmittedEvents);
    }

    private static async Task<EngineResult> CommitAsync(CommitCapitalCommand command, EngineContext context)
    {
        var aggregate = await context.LoadAggregate<CapitalAccountAggregate>(command.CapitalAccountId);
        aggregate.Commit(Guid.Parse(command.CapitalAccountId), command.Amount, command.CurrencyCode);
        await context.EmitEvents(aggregate);
        return EngineResult.Ok(context.EmittedEvents);
    }

    private static async Task<EngineResult> AllocateAsync(AllocateCapitalCommand command, EngineContext context)
    {
        var aggregate = await context.LoadAggregate<CapitalAccountAggregate>(command.CapitalAccountId);
        aggregate.Allocate(Guid.Parse(command.CapitalAccountId), command.AllocationTarget, command.Amount);
        await context.EmitEvents(aggregate);
        return EngineResult.Ok(context.EmittedEvents);
    }
}
