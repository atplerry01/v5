using Whycespace.Domain.EconomicSystem.Subject.Subject;
using Whycespace.Shared.Contracts.Economic.Subject.Subject;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Subject.Subject;

public sealed class RegisterEconomicSubjectHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RegisterEconomicSubjectCommand cmd)
            return Task.CompletedTask;

        var subjectType = Enum.Parse<SubjectType>(cmd.SubjectType, ignoreCase: false);
        var structuralRefType = Enum.Parse<StructuralRefType>(cmd.StructuralRefType, ignoreCase: false);
        var economicRefType = Enum.Parse<EconomicRefType>(cmd.EconomicRefType, ignoreCase: false);

        var aggregate = EconomicSubjectAggregate.Register(
            SubjectId.From(cmd.SubjectId),
            subjectType,
            new StructuralRef(structuralRefType, cmd.StructuralRefId),
            new EconomicRef(economicRefType, cmd.EconomicRefId));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
