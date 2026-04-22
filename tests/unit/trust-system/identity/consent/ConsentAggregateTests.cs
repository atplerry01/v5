using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.TrustSystem.Identity.Consent;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.TrustSystem.Identity.Consent;

public sealed class ConsentAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp FixedTs = new(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));

    private static ConsentId NewId(string seed) =>
        new(IdGen.Generate($"ConsentAggregateTests:{seed}:consent"));

    private static ConsentDescriptor DefaultDescriptor() =>
        new(IdGen.Generate("ConsentAggregateTests:identity-ref"), "data-processing", "MarketingAnalytics");

    [Fact]
    public void Grant_RaisesConsentGrantedEvent()
    {
        var id = NewId("Grant_Valid");
        var descriptor = DefaultDescriptor();
        var aggregate = ConsentAggregate.Grant(id, descriptor, FixedTs);
        var evt = Assert.IsType<ConsentGrantedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.ConsentId);
        Assert.Equal(descriptor.ConsentScope, evt.Descriptor.ConsentScope);
    }

    [Fact]
    public void Grant_SetsStatusToGranted()
    {
        var aggregate = ConsentAggregate.Grant(NewId("Status"), DefaultDescriptor(), FixedTs);
        Assert.Equal(ConsentStatus.Granted, aggregate.Status);
    }

    [Fact]
    public void Revoke_FromGranted_SetsStatusToRevoked()
    {
        var aggregate = ConsentAggregate.Grant(NewId("Revoke"), DefaultDescriptor(), FixedTs);
        aggregate.ClearDomainEvents();
        aggregate.Revoke();
        Assert.IsType<ConsentRevokedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(ConsentStatus.Revoked, aggregate.Status);
    }

    [Fact]
    public void Expire_FromGranted_SetsStatusToExpired()
    {
        var aggregate = ConsentAggregate.Grant(NewId("Expire"), DefaultDescriptor(), FixedTs);
        aggregate.ClearDomainEvents();
        aggregate.Expire();
        Assert.IsType<ConsentExpiredEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(ConsentStatus.Expired, aggregate.Status);
    }

    [Fact]
    public void Revoke_AfterRevoke_Throws()
    {
        var aggregate = ConsentAggregate.Grant(NewId("Revoke_Again"), DefaultDescriptor(), FixedTs);
        aggregate.Revoke();
        Assert.ThrowsAny<Exception>(() => aggregate.Revoke());
    }

    [Fact]
    public void Expire_AfterRevoke_Throws()
    {
        var aggregate = ConsentAggregate.Grant(NewId("Expire_After_Revoke"), DefaultDescriptor(), FixedTs);
        aggregate.Revoke();
        Assert.ThrowsAny<Exception>(() => aggregate.Expire());
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();
        var history = new object[] { new ConsentGrantedEvent(id, descriptor, FixedTs), new ConsentRevokedEvent(id) };
        var aggregate = (ConsentAggregate)Activator.CreateInstance(typeof(ConsentAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);
        Assert.Equal(id, aggregate.ConsentId);
        Assert.Equal(ConsentStatus.Revoked, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
