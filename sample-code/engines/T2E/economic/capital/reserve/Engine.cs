using Whycespace.Domain.EconomicSystem.Capital.Reserve;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Capital.Reserve;

public sealed class ReserveEngine : IEngine<ReserveCommand>
{
    private readonly ReservePolicyAdapter _policy = new();

    public async Task<EngineResult> ExecuteAsync(ReserveCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            CreateReserveCommand c => await CreateAsync(c, context),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private static async Task<EngineResult> CreateAsync(CreateReserveCommand command, EngineContext context)
    {
        var aggregate = await context.LoadAggregate<SPVAggregate>(command.Id);
        aggregate.Create(Guid.Parse(command.Id));
        await context.EmitEvents(aggregate);
        return EngineResult.Ok(context.EmittedEvents);
    }
}
