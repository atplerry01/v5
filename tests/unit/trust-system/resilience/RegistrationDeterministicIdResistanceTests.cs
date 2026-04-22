using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.TrustSystem.Identity.Registry;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.TrustSystem.Resilience;

/// <summary>
/// 2.8.22 — Duplicate registration resistance via deterministic IDs.
///
/// The registry uses seed-based ID generation (email + registrationType)
/// so repeated initiation requests with the same inputs produce the same
/// RegistryId, which the event store deduplicates at the aggregate level.
/// These tests verify the determinism invariant at the domain layer.
/// </summary>
public sealed class RegistrationDeterministicIdResistanceTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp FixedTs =
        new(new DateTimeOffset(2026, 4, 22, 0, 0, 0, TimeSpan.Zero));

    private static RegistryId DeterministicId(string email, string type) =>
        new(IdGen.Generate($"trust:identity:registry:{email}:{type}"));

    [Fact]
    public void SameEmailAndType_ProducesSameRegistryId()
    {
        var id1 = DeterministicId("alice@whycespace.com", "Individual");
        var id2 = DeterministicId("alice@whycespace.com", "Individual");

        Assert.Equal(id1, id2);
    }

    [Fact]
    public void DifferentEmail_ProducesDifferentRegistryId()
    {
        var id1 = DeterministicId("alice@whycespace.com", "Individual");
        var id2 = DeterministicId("bob@whycespace.com", "Individual");

        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void DifferentType_ProducesDifferentRegistryId()
    {
        var id1 = DeterministicId("alice@whycespace.com", "Individual");
        var id2 = DeterministicId("alice@whycespace.com", "Organisation");

        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void RegistryAggregate_Initiate_IsDeterministicUnderReplay()
    {
        var id = DeterministicId("replay@whycespace.com", "Individual");
        var descriptor = new RegistrationDescriptor("replay@whycespace.com", "Individual");

        var first = RegistryAggregate.Initiate(id, descriptor, FixedTs);
        var second = RegistryAggregate.Initiate(id, descriptor, FixedTs);

        var e1 = (RegistrationInitiatedEvent)first.DomainEvents[0];
        var e2 = (RegistrationInitiatedEvent)second.DomainEvents[0];

        Assert.Equal(e1.RegistryId, e2.RegistryId);
        Assert.Equal(e1.Descriptor.Email, e2.Descriptor.Email);
        Assert.Equal(e1.Descriptor.RegistrationType, e2.Descriptor.RegistrationType);
        Assert.Equal(e1.InitiatedAt, e2.InitiatedAt);
    }

    [Fact]
    public void RegistryAggregate_StateMachine_AllValidTransitionsSucceed()
    {
        var id = DeterministicId("state@whycespace.com", "Individual");
        var descriptor = new RegistrationDescriptor("state@whycespace.com", "Individual");

        var initiatedToActivated = RegistryAggregate.Initiate(id, descriptor, FixedTs);
        initiatedToActivated.Verify();
        initiatedToActivated.Activate();
        Assert.Equal(RegistrationStatus.Activated, initiatedToActivated.Status);

        var initiatedToRejected = RegistryAggregate.Initiate(id, descriptor, FixedTs);
        initiatedToRejected.Reject("Anti-fraud check failed.");
        Assert.Equal(RegistrationStatus.Rejected, initiatedToRejected.Status);

        var initiatedToLocked = RegistryAggregate.Initiate(id, descriptor, FixedTs);
        initiatedToLocked.LockOut("Throttle limit exceeded.");
        Assert.Equal(RegistrationStatus.Locked, initiatedToLocked.Status);

        var verifiedToRejected = RegistryAggregate.Initiate(id, descriptor, FixedTs);
        verifiedToRejected.Verify();
        verifiedToRejected.Reject("Failed secondary review.");
        Assert.Equal(RegistrationStatus.Rejected, verifiedToRejected.Status);

        var verifiedToLocked = RegistryAggregate.Initiate(id, descriptor, FixedTs);
        verifiedToLocked.Verify();
        verifiedToLocked.LockOut("Suspicious pattern after verification.");
        Assert.Equal(RegistrationStatus.Locked, verifiedToLocked.Status);
    }

    [Fact]
    public void RegistryAggregate_StateMachine_AllInvalidTransitionsThrow()
    {
        var id = DeterministicId("invalid@whycespace.com", "Individual");
        var descriptor = new RegistrationDescriptor("invalid@whycespace.com", "Individual");

        var activated = RegistryAggregate.Initiate(id, descriptor, FixedTs);
        activated.Verify();
        activated.Activate();
        Assert.ThrowsAny<Exception>(() => activated.Reject("Too late."));
        Assert.ThrowsAny<Exception>(() => activated.LockOut("Too late."));

        var rejected = RegistryAggregate.Initiate(id, descriptor, FixedTs);
        rejected.Reject("Initial rejection.");
        Assert.ThrowsAny<Exception>(() => rejected.Reject("Double rejection."));

        var locked = RegistryAggregate.Initiate(id, descriptor, FixedTs);
        locked.LockOut("First lock.");
        Assert.ThrowsAny<Exception>(() => locked.LockOut("Second lock."));

        var notVerified = RegistryAggregate.Initiate(id, descriptor, FixedTs);
        Assert.ThrowsAny<Exception>(() => notVerified.Activate());
    }
}
