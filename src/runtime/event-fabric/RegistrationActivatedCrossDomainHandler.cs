using Whycespace.Shared.Contracts.Economic.Subject.Subject;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Trust.Identity.Registry;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Structural.Humancapital.Participant;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Runtime.EventFabric;

/// <summary>
/// Cross-domain bridge: fires when a registration is activated and establishes
/// the actor's standing in the structural and economic systems.
///
/// On RegistrationActivatedEvent:
///   1. RegisterParticipantCommand → structural/humancapital/participant
///      (places the actor in the structural topology)
///   2. RegisterEconomicSubjectCommand → economic/subject/subject
///      (gives the actor economic standing for capital/ledger flows)
///
/// Both IDs are derived deterministically from the RegistryId so they are
/// stable across replays and never require coordination. Envelope-level
/// idempotency (IIdempotencyStore) prevents double-dispatch on Kafka
/// redelivery. Propagation failure does NOT roll back the registry —
/// eventual consistency with explicit reconciliation per 2.8.18 spec.
/// </summary>
public sealed class RegistrationActivatedCrossDomainHandler
{
    private const string IdempotencyKeyPrefix = "registration-activated-cross-domain";

    private static readonly DomainRoute ParticipantRoute =
        new("structural", "humancapital", "participant");

    private static readonly DomainRoute SubjectRoute =
        new("economic", "subject", "subject");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly IIdempotencyStore _idempotencyStore;

    public RegistrationActivatedCrossDomainHandler(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IIdempotencyStore idempotencyStore)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _idempotencyStore = idempotencyStore;
    }

    public async Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        if (envelope.Payload is not RegistrationActivatedEventSchema activated)
            return;

        var registryId = activated.AggregateId;
        var idempotencyKey = $"{IdempotencyKeyPrefix}:{envelope.EventId}";
        var claimed = await _idempotencyStore.TryClaimAsync(idempotencyKey, cancellationToken);
        if (!claimed)
            return;

        try
        {
            await DispatchCrossDomainAsync(registryId, cancellationToken);
        }
        catch
        {
            await _idempotencyStore.ReleaseAsync(idempotencyKey, cancellationToken);
            throw;
        }
    }

    private async Task DispatchCrossDomainAsync(Guid registryId, CancellationToken ct)
    {
        var participantId = _idGenerator.Generate(
            $"structural:humancapital:participant:{registryId}");

        var subjectId = _idGenerator.Generate(
            $"economic:subject:subject:{registryId}");

        // 1. Place actor in structural system
        await _dispatcher.DispatchSystemAsync(
            new RegisterParticipantCommand(participantId),
            ParticipantRoute,
            ct);

        // 2. Establish actor in economic system
        await _dispatcher.DispatchSystemAsync(
            new RegisterEconomicSubjectCommand(
                SubjectId: subjectId,
                SubjectType: "Participant",
                StructuralRefType: "participant",
                StructuralRefId: participantId.ToString("N"),
                EconomicRefType: "registry",
                EconomicRefId: registryId.ToString("N")),
            SubjectRoute,
            ct);
    }
}
