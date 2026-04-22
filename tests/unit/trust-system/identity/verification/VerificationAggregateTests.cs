using Whycespace.Domain.TrustSystem.Identity.Verification;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.TrustSystem.Identity.Verification;

public sealed class VerificationAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static VerificationId NewId(string seed) =>
        new(IdGen.Generate($"VerificationAggregateTests:{seed}:verification"));

    private static VerificationSubject DefaultSubject(string seed) =>
        new(IdGen.Generate($"VerificationAggregateTests:{seed}:identity"), "email");

    [Fact]
    public void Initiate_RaisesVerificationInitiatedEvent()
    {
        var id = NewId("Initiate_Valid");
        var subject = DefaultSubject("Initiate_Valid");

        var aggregate = VerificationAggregate.Initiate(id, subject);

        var evt = Assert.IsType<VerificationInitiatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.VerificationId);
        Assert.Equal(subject.ClaimType, evt.Subject.ClaimType);
    }

    [Fact]
    public void Initiate_SetsStatusToInitiated()
    {
        var aggregate = VerificationAggregate.Initiate(NewId("State"), DefaultSubject("State"));

        Assert.Equal(VerificationStatus.Initiated, aggregate.Status);
    }

    [Fact]
    public void Initiate_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var subject = DefaultSubject("Stable");

        var v1 = VerificationAggregate.Initiate(id, subject);
        var v2 = VerificationAggregate.Initiate(id, subject);

        Assert.Equal(
            ((VerificationInitiatedEvent)v1.DomainEvents[0]).VerificationId.Value,
            ((VerificationInitiatedEvent)v2.DomainEvents[0]).VerificationId.Value);
    }

    [Fact]
    public void Pass_FromInitiated_SetsStatusToPassed()
    {
        var aggregate = VerificationAggregate.Initiate(NewId("Pass"), DefaultSubject("Pass"));
        aggregate.ClearDomainEvents();

        aggregate.Pass();

        Assert.IsType<VerificationPassedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(VerificationStatus.Passed, aggregate.Status);
    }

    [Fact]
    public void Fail_FromInitiated_SetsStatusToFailed()
    {
        var aggregate = VerificationAggregate.Initiate(NewId("Fail"), DefaultSubject("Fail"));
        aggregate.ClearDomainEvents();

        aggregate.Fail();

        Assert.IsType<VerificationFailedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(VerificationStatus.Failed, aggregate.Status);
    }

    [Fact]
    public void Pass_AfterPass_Throws()
    {
        var aggregate = VerificationAggregate.Initiate(NewId("Pass_Again"), DefaultSubject("Pass_Again"));
        aggregate.Pass();

        Assert.ThrowsAny<Exception>(() => aggregate.Pass());
    }

    [Fact]
    public void Fail_AfterPass_Throws()
    {
        var aggregate = VerificationAggregate.Initiate(NewId("Fail_AfterPass"), DefaultSubject("Fail_AfterPass"));
        aggregate.Pass();

        Assert.ThrowsAny<Exception>(() => aggregate.Fail());
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");
        var subject = DefaultSubject("History");

        var history = new object[]
        {
            new VerificationInitiatedEvent(id, subject)
        };

        var aggregate = (VerificationAggregate)Activator.CreateInstance(typeof(VerificationAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.VerificationId);
        Assert.Equal(subject.ClaimType, aggregate.Subject.ClaimType);
        Assert.Equal(VerificationStatus.Initiated, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
