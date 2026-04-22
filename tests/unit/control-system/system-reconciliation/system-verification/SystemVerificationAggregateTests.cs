using Whycespace.Domain.ControlSystem.SystemReconciliation.SystemVerification;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.SystemReconciliation.SystemVerification;

public sealed class SystemVerificationAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly DateTimeOffset InitiatedAt = new(2026, 4, 22, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset PassedAt = new(2026, 4, 22, 10, 5, 0, TimeSpan.Zero);

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static SystemVerificationId NewId(string seed) =>
        new(Hex64($"SystemVerificationTests:{seed}:verif"));

    [Fact]
    public void Initiate_RaisesSystemVerificationInitiatedEvent()
    {
        var id = NewId("Initiate");

        var aggregate = SystemVerificationAggregate.Initiate(id, "audit-engine", InitiatedAt);

        var evt = Assert.IsType<SystemVerificationInitiatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal("audit-engine", evt.TargetSystem);
        Assert.Equal(InitiatedAt, evt.InitiatedAt);
    }

    [Fact]
    public void Initiate_SetsStatusToInitiated()
    {
        var aggregate = SystemVerificationAggregate.Initiate(NewId("State"), "system", InitiatedAt);

        Assert.Equal(VerificationStatus.Initiated, aggregate.Status);
        Assert.Null(aggregate.FailureReason);
    }

    [Fact]
    public void Initiate_WithEmptyTargetSystem_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            SystemVerificationAggregate.Initiate(NewId("EmptyTarget"), "", InitiatedAt));
    }

    [Fact]
    public void Pass_RaisesSystemVerificationPassedEvent()
    {
        var aggregate = SystemVerificationAggregate.Initiate(NewId("Pass"), "audit-engine", InitiatedAt);
        aggregate.ClearDomainEvents();

        aggregate.Pass(PassedAt);

        Assert.IsType<SystemVerificationPassedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(VerificationStatus.Passed, aggregate.Status);
        Assert.Null(aggregate.FailureReason);
    }

    [Fact]
    public void Pass_AlreadyTerminated_Throws()
    {
        var aggregate = SystemVerificationAggregate.Initiate(NewId("PassTerminated"), "system", InitiatedAt);
        aggregate.Pass(PassedAt);

        Assert.ThrowsAny<Exception>(() => aggregate.Pass(PassedAt.AddMinutes(1)));
    }

    [Fact]
    public void Fail_RaisesSystemVerificationFailedEvent()
    {
        var aggregate = SystemVerificationAggregate.Initiate(NewId("Fail"), "audit-engine", InitiatedAt);
        aggregate.ClearDomainEvents();

        aggregate.Fail("integrity hash mismatch", PassedAt);

        var evt = Assert.IsType<SystemVerificationFailedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal("integrity hash mismatch", evt.FailureReason);
        Assert.Equal(VerificationStatus.Failed, aggregate.Status);
        Assert.Equal("integrity hash mismatch", aggregate.FailureReason);
    }

    [Fact]
    public void Fail_WithEmptyFailureReason_Throws()
    {
        var aggregate = SystemVerificationAggregate.Initiate(NewId("EmptyReason"), "system", InitiatedAt);

        Assert.ThrowsAny<Exception>(() => aggregate.Fail("", PassedAt));
    }

    [Fact]
    public void Fail_AlreadyTerminated_Throws()
    {
        var aggregate = SystemVerificationAggregate.Initiate(NewId("FailTerminated"), "system", InitiatedAt);
        aggregate.Fail("reason", PassedAt);

        Assert.ThrowsAny<Exception>(() => aggregate.Fail("reason-2", PassedAt.AddMinutes(1)));
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");

        var history = new object[]
        {
            new SystemVerificationInitiatedEvent(id, "audit-engine", InitiatedAt)
        };
        var aggregate = (SystemVerificationAggregate)Activator.CreateInstance(typeof(SystemVerificationAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(VerificationStatus.Initiated, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
