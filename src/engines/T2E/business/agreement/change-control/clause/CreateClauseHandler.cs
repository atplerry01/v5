using Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Clause;
using Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Clause;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Agreement.ChangeControl.Clause;

public sealed class CreateClauseHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateClauseCommand cmd)
            return Task.CompletedTask;

        if (!Enum.TryParse<ClauseType>(cmd.ClauseType, ignoreCase: true, out var parsedType))
            throw ClauseErrors.InvalidClauseType();

        var aggregate = ClauseAggregate.Create(new ClauseId(cmd.ClauseId), parsedType);
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
