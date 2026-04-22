using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.TrustSystem.Identity.Trust;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.TrustSystem.Identity.Trust;

public sealed class TrustAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp FixedTs = new(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));

    private static TrustId NewId(string seed) =>
        new(IdGen.Generate($"TrustAggregateTests:{seed}:trust"));

    private static TrustDescriptor DefaultDescriptor() =>
        new(IdGen.Generate("TrustAggregateTests:identity-ref"), "Behavioural", 0.75m);

    [Fact]
    public void Assess_RaisesTrustAssessedEvent()
    {
        var id = NewId("Assess_Valid");
        var descriptor = DefaultDescriptor();
        var aggregate = TrustAggregate.Assess(id, descriptor, FixedTs);
        var evt = Assert.IsType<TrustAssessedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.TrustId);
        Assert.Equal(descriptor.Score, evt.Descriptor.Score);
    }

    [Fact]
    public void Assess_SetsStatusToAssessed()
    {
        var aggregate = TrustAggregate.Assess(NewId("Status"), DefaultDescriptor(), FixedTs);
        Assert.Equal(TrustStatus.Assessed, aggregate.Status);
    }

    [Fact]
    public void Activate_FromAssessed_SetsStatusToActive()
    {
        var aggregate = TrustAggregate.Assess(NewId("Activate"), DefaultDescriptor(), FixedTs);
        aggregate.ClearDomainEvents();
        aggregate.Activate();
        Assert.IsType<TrustActivatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(TrustStatus.Active, aggregate.Status);
    }

    [Fact]
    public void Suspend_FromActive_SetsStatusToSuspended()
    {
        var aggregate = TrustAggregate.Assess(NewId("Suspend"), DefaultDescriptor(), FixedTs);
        aggregate.Activate();
        aggregate.ClearDomainEvents();
        aggregate.Suspend();
        Assert.Equal(TrustStatus.Suspended, aggregate.Status);
    }

    [Fact]
    public void Revoke_FromActive_SetsStatusToRevoked()
    {
        var aggregate = TrustAggregate.Assess(NewId("Revoke_Active"), DefaultDescriptor(), FixedTs);
        aggregate.Activate();
        aggregate.ClearDomainEvents();
        aggregate.Revoke();
        Assert.Equal(TrustStatus.Revoked, aggregate.Status);
    }

    [Fact]
    public void Revoke_FromSuspended_SetsStatusToRevoked()
    {
        var aggregate = TrustAggregate.Assess(NewId("Revoke_Suspended"), DefaultDescriptor(), FixedTs);
        aggregate.Activate();
        aggregate.Suspend();
        aggregate.ClearDomainEvents();
        aggregate.Revoke();
        Assert.Equal(TrustStatus.Revoked, aggregate.Status);
    }

    [Fact]
    public void Revoke_FromAssessed_Throws()
    {
        var aggregate = TrustAggregate.Assess(NewId("Revoke_Assessed"), DefaultDescriptor(), FixedTs);
        Assert.ThrowsAny<Exception>(() => aggregate.Revoke());
    }

    [Fact]
    public void Suspend_FromAssessed_Throws()
    {
        var aggregate = TrustAggregate.Assess(NewId("Suspend_Assessed"), DefaultDescriptor(), FixedTs);
        Assert.ThrowsAny<Exception>(() => aggregate.Suspend());
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();
        var history = new object[] { new TrustAssessedEvent(id, descriptor, FixedTs), new TrustActivatedEvent(id) };
        var aggregate = (TrustAggregate)Activator.CreateInstance(typeof(TrustAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);
        Assert.Equal(id, aggregate.TrustId);
        Assert.Equal(TrustStatus.Active, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void TrustDescriptor_ScoreOutOfRange_Throws()
    {
        var identityRef = IdGen.Generate("TrustAggregateTests:score-ref");
        Assert.ThrowsAny<Exception>(() => new TrustDescriptor(identityRef, "Test", 1.5m));
    }
}
