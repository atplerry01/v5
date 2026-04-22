using Whycespace.Domain.TrustSystem.Identity.Credential;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.TrustSystem.Identity.Credential;

public sealed class CredentialAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static CredentialId NewId(string seed) =>
        new(IdGen.Generate($"CredentialAggregateTests:{seed}:credential"));

    private static CredentialDescriptor DefaultDescriptor(string seed) =>
        new(IdGen.Generate($"CredentialAggregateTests:{seed}:identity"), "Password");

    [Fact]
    public void Issue_RaisesCredentialIssuedEvent()
    {
        var id = NewId("Issue_Valid");
        var descriptor = DefaultDescriptor("Issue_Valid");

        var aggregate = CredentialAggregate.Issue(id, descriptor);

        var evt = Assert.IsType<CredentialIssuedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.CredentialId);
        Assert.Equal(descriptor.CredentialType, evt.Descriptor.CredentialType);
    }

    [Fact]
    public void Issue_SetsStatusToIssued()
    {
        var aggregate = CredentialAggregate.Issue(NewId("State"), DefaultDescriptor("State"));

        Assert.Equal(CredentialStatus.Issued, aggregate.Status);
    }

    [Fact]
    public void Issue_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var descriptor = DefaultDescriptor("Stable");

        var c1 = CredentialAggregate.Issue(id, descriptor);
        var c2 = CredentialAggregate.Issue(id, descriptor);

        Assert.Equal(
            ((CredentialIssuedEvent)c1.DomainEvents[0]).CredentialId.Value,
            ((CredentialIssuedEvent)c2.DomainEvents[0]).CredentialId.Value);
    }

    [Fact]
    public void Activate_FromIssued_SetsStatusToActive()
    {
        var aggregate = CredentialAggregate.Issue(NewId("Activate"), DefaultDescriptor("Activate"));
        aggregate.ClearDomainEvents();

        aggregate.Activate();

        Assert.IsType<CredentialActivatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(CredentialStatus.Active, aggregate.Status);
    }

    [Fact]
    public void Revoke_FromActive_SetsStatusToRevoked()
    {
        var aggregate = CredentialAggregate.Issue(NewId("Revoke"), DefaultDescriptor("Revoke"));
        aggregate.Activate();
        aggregate.ClearDomainEvents();

        aggregate.Revoke();

        Assert.IsType<CredentialRevokedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(CredentialStatus.Revoked, aggregate.Status);
    }

    [Fact]
    public void Revoke_AfterRevoke_Throws()
    {
        var aggregate = CredentialAggregate.Issue(NewId("Revoke_Again"), DefaultDescriptor("Revoke_Again"));
        aggregate.Activate();
        aggregate.Revoke();

        Assert.ThrowsAny<Exception>(() => aggregate.Revoke());
    }

    [Fact]
    public void Activate_AfterRevoke_Throws()
    {
        var aggregate = CredentialAggregate.Issue(NewId("Activate_Revoked"), DefaultDescriptor("Activate_Revoked"));
        aggregate.Activate();
        aggregate.Revoke();

        Assert.ThrowsAny<Exception>(() => aggregate.Activate());
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor("History");

        var history = new object[]
        {
            new CredentialIssuedEvent(id, descriptor)
        };

        var aggregate = (CredentialAggregate)Activator.CreateInstance(typeof(CredentialAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(CredentialStatus.Issued, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
