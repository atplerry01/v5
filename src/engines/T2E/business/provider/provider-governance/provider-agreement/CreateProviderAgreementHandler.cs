using Whycespace.Domain.BusinessSystem.Provider.ProviderGovernance.ProviderAgreement;
using Whycespace.Domain.BusinessSystem.Shared.Time;
using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Shared.Contracts.Business.Provider.ProviderGovernance.ProviderAgreement;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Provider.ProviderGovernance.ProviderAgreement;

public sealed class CreateProviderAgreementHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateProviderAgreementCommand cmd)
            return Task.CompletedTask;

        ContractRef? contract = cmd.ContractId.HasValue
            ? new ContractRef(cmd.ContractId.Value)
            : null;

        TimeWindow? effective = cmd.EffectiveStartsAt.HasValue
            ? new TimeWindow(cmd.EffectiveStartsAt.Value, cmd.EffectiveEndsAt)
            : null;

        var aggregate = ProviderAgreementAggregate.Create(
            new ProviderAgreementId(cmd.ProviderAgreementId),
            new ClusterProviderRef(cmd.ProviderId),
            contract,
            effective);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
