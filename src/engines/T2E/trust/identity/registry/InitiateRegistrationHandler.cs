using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.TrustSystem.Identity.Registry;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Observability;
using Whycespace.Shared.Contracts.Trust.Identity.Registry;

namespace Whycespace.Engines.T2E.Trust.Identity.Registry;

public sealed class InitiateRegistrationHandler : IEngine
{
    private readonly ITrustMetrics _metrics;

    public InitiateRegistrationHandler(ITrustMetrics metrics) => _metrics = metrics;

    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not InitiateRegistrationCommand cmd)
            return Task.CompletedTask;

        var aggregate = RegistryAggregate.Initiate(
            new RegistryId(cmd.RegistryId),
            new RegistrationDescriptor(cmd.Email, cmd.RegistrationType),
            new Timestamp(cmd.InitiatedAt));

        context.EmitEvents(aggregate.DomainEvents);
        _metrics.RecordRegistrationInitiated(cmd.RegistrationType);
        return Task.CompletedTask;
    }
}
