using Whycespace.Domain.EconomicSystem.Capital.Binding;
using Whycespace.Domain.SharedKernel.Primitive.Identity;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Capital.Binding;

public sealed class BindingEngine : IEngine<BindingCommand>
{
    private readonly BindingPolicyAdapter _policy = new();

    public async Task<EngineResult> ExecuteAsync(BindingCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            CreateBindingCommand c => await CreateAsync(c, context),
            RevokeBindingCommand c => await RevokeAsync(c, context),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private static async Task<EngineResult> CreateAsync(CreateBindingCommand command, EngineContext context)
    {
        var aggregate = await context.LoadAggregate<IdentityEconomicBindingAggregate>(command.Id);
        aggregate.Create(Guid.Parse(command.Id), new IdentityId(Guid.Parse(command.IdentityId)), Guid.Parse(command.WalletId));
        await context.EmitEvents(aggregate);
        return EngineResult.Ok(context.EmittedEvents);
    }

    private static async Task<EngineResult> RevokeAsync(RevokeBindingCommand command, EngineContext context)
    {
        var aggregate = await context.LoadAggregate<IdentityEconomicBindingAggregate>(command.BindingId);
        aggregate.Revoke();
        await context.EmitEvents(aggregate);
        return EngineResult.Ok(context.EmittedEvents);
    }
}
