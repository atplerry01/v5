using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.TrustSystem.Identity.Registry;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.TrustSystem.Identity.Registry;

public sealed class RegistryAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp FixedTs = new(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));

    private static RegistryId NewId(string seed) =>
        new(IdGen.Generate($"RegistryAggregateTests:{seed}:registry"));

    private static RegistrationDescriptor DefaultDescriptor() =>
        new("alice@whycespace.com", "Individual");

    [Fact]
    public void Initiate_RaisesRegistrationInitiatedEvent()
    {
        var id = NewId("Initiate_Valid");
        var descriptor = DefaultDescriptor();

        var aggregate = RegistryAggregate.Initiate(id, descriptor, FixedTs);

        var evt = Assert.IsType<RegistrationInitiatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.RegistryId);
        Assert.Equal(descriptor.Email, evt.Descriptor.Email);
        Assert.Equal(descriptor.RegistrationType, evt.Descriptor.RegistrationType);
        Assert.Equal(FixedTs, evt.InitiatedAt);
    }

    [Fact]
    public void Initiate_SetsStatusToInitiated()
    {
        var aggregate = RegistryAggregate.Initiate(NewId("Status_Initiated"), DefaultDescriptor(), FixedTs);

        Assert.Equal(RegistrationStatus.Initiated, aggregate.Status);
    }

    [Fact]
    public void Verify_FromInitiated_SetsStatusToVerified()
    {
        var aggregate = RegistryAggregate.Initiate(NewId("Verify_Initiated"), DefaultDescriptor(), FixedTs);
        aggregate.ClearDomainEvents();

        aggregate.Verify();

        Assert.IsType<RegistrationVerifiedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(RegistrationStatus.Verified, aggregate.Status);
    }

    [Fact]
    public void Activate_FromVerified_SetsStatusToActivated()
    {
        var aggregate = RegistryAggregate.Initiate(NewId("Activate_Verified"), DefaultDescriptor(), FixedTs);
        aggregate.Verify();
        aggregate.ClearDomainEvents();

        aggregate.Activate();

        Assert.IsType<RegistrationActivatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(RegistrationStatus.Activated, aggregate.Status);
    }

    [Fact]
    public void Reject_FromInitiated_SetsStatusToRejected()
    {
        var aggregate = RegistryAggregate.Initiate(NewId("Reject_Initiated"), DefaultDescriptor(), FixedTs);
        aggregate.ClearDomainEvents();

        aggregate.Reject("Duplicate registration detected.");

        var evt = Assert.IsType<RegistrationRejectedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(RegistrationStatus.Rejected, aggregate.Status);
        Assert.Equal("Duplicate registration detected.", evt.Reason);
    }

    [Fact]
    public void Reject_FromVerified_SetsStatusToRejected()
    {
        var aggregate = RegistryAggregate.Initiate(NewId("Reject_Verified"), DefaultDescriptor(), FixedTs);
        aggregate.Verify();
        aggregate.ClearDomainEvents();

        aggregate.Reject("Failed secondary check.");

        Assert.Equal(RegistrationStatus.Rejected, aggregate.Status);
    }

    [Fact]
    public void Verify_FromVerified_Throws()
    {
        var aggregate = RegistryAggregate.Initiate(NewId("Verify_Again"), DefaultDescriptor(), FixedTs);
        aggregate.Verify();

        Assert.ThrowsAny<Exception>(() => aggregate.Verify());
    }

    [Fact]
    public void Activate_FromInitiated_Throws()
    {
        var aggregate = RegistryAggregate.Initiate(NewId("Activate_Initiated"), DefaultDescriptor(), FixedTs);

        Assert.ThrowsAny<Exception>(() => aggregate.Activate());
    }

    [Fact]
    public void Reject_FromActivated_Throws()
    {
        var aggregate = RegistryAggregate.Initiate(NewId("Reject_Activated"), DefaultDescriptor(), FixedTs);
        aggregate.Verify();
        aggregate.Activate();

        Assert.ThrowsAny<Exception>(() => aggregate.Reject("Too late."));
    }

    [Fact]
    public void Reject_WithEmptyReason_Throws()
    {
        var aggregate = RegistryAggregate.Initiate(NewId("Reject_EmptyReason"), DefaultDescriptor(), FixedTs);

        Assert.ThrowsAny<Exception>(() => aggregate.Reject(""));
    }

    [Fact]
    public void Descriptor_NormalizesEmailToLowercase()
    {
        var aggregate = RegistryAggregate.Initiate(NewId("Email_Normalize"), new RegistrationDescriptor("ALICE@Whycespace.COM", "Individual"), FixedTs);

        Assert.Equal("alice@whycespace.com", aggregate.Descriptor.Email);
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();

        var history = new object[]
        {
            new RegistrationInitiatedEvent(id, descriptor, FixedTs),
            new RegistrationVerifiedEvent(id)
        };

        var aggregate = (RegistryAggregate)Activator.CreateInstance(typeof(RegistryAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.RegistryId);
        Assert.Equal(descriptor.Email, aggregate.Descriptor.Email);
        Assert.Equal(RegistrationStatus.Verified, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void LoadFromHistory_MatchesDirectConstruction()
    {
        var id = NewId("Replay");
        var descriptor = DefaultDescriptor();

        var direct = RegistryAggregate.Initiate(id, descriptor, FixedTs);
        direct.Verify();
        direct.Activate();

        var replayed = (RegistryAggregate)Activator.CreateInstance(typeof(RegistryAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(direct.DomainEvents);

        Assert.Equal(direct.RegistryId, replayed.RegistryId);
        Assert.Equal(direct.Descriptor.Email, replayed.Descriptor.Email);
        Assert.Equal(direct.Status, replayed.Status);
    }

    // ── Lockout tests (2.8.19 / 2.8.21) ─────────────────────────────────

    [Fact]
    public void LockOut_FromInitiated_SetsStatusToLocked()
    {
        var aggregate = RegistryAggregate.Initiate(NewId("Lock_Initiated"), DefaultDescriptor(), FixedTs);
        aggregate.ClearDomainEvents();

        aggregate.LockOut("Abuse pattern detected.");

        var evt = Assert.IsType<RegistrationLockedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(RegistrationStatus.Locked, aggregate.Status);
        Assert.Equal("Abuse pattern detected.", evt.Reason);
    }

    [Fact]
    public void LockOut_FromVerified_SetsStatusToLocked()
    {
        var aggregate = RegistryAggregate.Initiate(NewId("Lock_Verified"), DefaultDescriptor(), FixedTs);
        aggregate.Verify();
        aggregate.ClearDomainEvents();

        aggregate.LockOut("Suspicious activity after verification.");

        Assert.Equal(RegistrationStatus.Locked, aggregate.Status);
    }

    [Fact]
    public void LockOut_FromActivated_Throws()
    {
        var aggregate = RegistryAggregate.Initiate(NewId("Lock_Activated"), DefaultDescriptor(), FixedTs);
        aggregate.Verify();
        aggregate.Activate();

        Assert.ThrowsAny<Exception>(() => aggregate.LockOut("Too late."));
    }

    [Fact]
    public void LockOut_WhenAlreadyLocked_Throws()
    {
        var aggregate = RegistryAggregate.Initiate(NewId("Lock_Again"), DefaultDescriptor(), FixedTs);
        aggregate.LockOut("First lock.");

        Assert.ThrowsAny<Exception>(() => aggregate.LockOut("Second lock."));
    }

    [Fact]
    public void LockOut_WithEmptyReason_Throws()
    {
        var aggregate = RegistryAggregate.Initiate(NewId("Lock_EmptyReason"), DefaultDescriptor(), FixedTs);

        Assert.ThrowsAny<Exception>(() => aggregate.LockOut(""));
    }

    [Fact]
    public void LockOut_Replay_PreservesState()
    {
        var id = NewId("Lock_Replay");
        var descriptor = DefaultDescriptor();
        var direct = RegistryAggregate.Initiate(id, descriptor, FixedTs);
        direct.LockOut("Replay test lock reason.");

        var replayed = (RegistryAggregate)Activator.CreateInstance(typeof(RegistryAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(direct.DomainEvents);

        Assert.Equal(RegistrationStatus.Locked, replayed.Status);
        Assert.Equal(id, replayed.RegistryId);
        Assert.Equal(descriptor.Email, replayed.Descriptor.Email);
    }
}
