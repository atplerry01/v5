using Whycespace.Domain.PlatformSystem.Schema.Contract;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Schema.Contract;

namespace Whycespace.Engines.T2E.Platform.Schema.Contract;

public sealed class RegisterContractHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RegisterContractCommand cmd)
            return Task.CompletedTask;

        var aggregate = ContractAggregate.Register(
            new ContractId(cmd.ContractId),
            cmd.ContractName,
            new DomainRoute(cmd.PublisherClassification, cmd.PublisherContext, cmd.PublisherDomain),
            cmd.SchemaRef,
            cmd.SchemaVersion,
            new Timestamp(cmd.RegisteredAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
