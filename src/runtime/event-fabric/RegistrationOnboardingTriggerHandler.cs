using Whycespace.Domain.TrustSystem.Identity.Registry;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Trust.Identity.Registry.Workflow;

namespace Whycespace.Runtime.EventFabric;

public sealed class RegistrationOnboardingTriggerHandler
{
    private static readonly DomainRoute RegistryRoute = new("trust", "identity", "registry");

    private readonly IWorkflowDispatcher _workflowDispatcher;

    public RegistrationOnboardingTriggerHandler(IWorkflowDispatcher workflowDispatcher)
    {
        _workflowDispatcher = workflowDispatcher;
    }

    public async Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        if (envelope.EventType != "RegistrationInitiatedEvent")
            return;

        if (envelope.Payload is not RegistrationInitiatedEvent initiated)
            return;

        var intent = new RegistrationOnboardingIntent(
            initiated.RegistryId.Value,
            initiated.Descriptor.Email,
            initiated.Descriptor.RegistrationType);

        _ = cancellationToken;

        await _workflowDispatcher.StartWorkflowAsync(
            RegistrationOnboardingWorkflowNames.Onboard,
            intent,
            RegistryRoute);
    }
}
