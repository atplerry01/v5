using Whycespace.Runtime.Middleware.Policy.Loaders;
using Whycespace.Shared.Contracts.Economic.Ledger.Obligation;
using Whycespace.Shared.Contracts.Economic.Ledger.Treasury;
using Whycespace.Shared.Contracts.Infrastructure.Policy;

namespace Whycespace.Tests.Integration.Phase11;

/// <summary>
/// Phase 11 B3 — validation of <see cref="CompositeAggregateStateLoader"/>
/// dispatch. Uses lightweight scripted <see cref="IAggregateStateLoader"/>
/// stubs registered with synthetic handles-predicates, so the composite's
/// routing logic is tested in isolation from the real per-aggregate loaders
/// (which have their own test files).
/// </summary>
public sealed class CompositeAggregateStateLoaderTests
{
    private sealed class ScriptedLoader : IAggregateStateLoader
    {
        public int CallCount { get; private set; }
        public Type? LastCommandType { get; private set; }
        public Guid LastAggregateId { get; private set; }
        public object? Return { get; set; }

        public Task<object?> LoadSnapshotAsync(
            Type commandType, Guid aggregateId, CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastCommandType = commandType;
            LastAggregateId = aggregateId;
            return Task.FromResult(Return);
        }
    }

    [Fact]
    public async Task MatchingRoute_DelegatesToFirstMatchingLoader()
    {
        var obligationStub = new ScriptedLoader { Return = "obligation-snapshot" };
        var treasuryStub = new ScriptedLoader { Return = "treasury-snapshot" };

        var composite = new CompositeAggregateStateLoader(new[]
        {
            new CompositeAggregateStateLoader.Route(ObligationStateLoader.Handles, obligationStub),
            new CompositeAggregateStateLoader.Route(TreasuryStateLoader.Handles, treasuryStub),
        });

        var aggregateId = Guid.Parse("00000000-0000-0000-0000-000000000d01");

        var result = await composite.LoadSnapshotAsync(
            typeof(FulfilObligationCommand), aggregateId);

        Assert.Equal("obligation-snapshot", result);
        Assert.Equal(1, obligationStub.CallCount);
        Assert.Equal(0, treasuryStub.CallCount);
        Assert.Equal(typeof(FulfilObligationCommand), obligationStub.LastCommandType);
        Assert.Equal(aggregateId, obligationStub.LastAggregateId);
    }

    [Fact]
    public async Task MatchingRoute_TreasuryCommand_DelegatesToTreasuryLoader()
    {
        var obligationStub = new ScriptedLoader();
        var treasuryStub = new ScriptedLoader { Return = "treasury-snapshot" };

        var composite = new CompositeAggregateStateLoader(new[]
        {
            new CompositeAggregateStateLoader.Route(ObligationStateLoader.Handles, obligationStub),
            new CompositeAggregateStateLoader.Route(TreasuryStateLoader.Handles, treasuryStub),
        });

        var result = await composite.LoadSnapshotAsync(
            typeof(AllocateFundsCommand), Guid.NewGuid());

        Assert.Equal("treasury-snapshot", result);
        Assert.Equal(0, obligationStub.CallCount);
        Assert.Equal(1, treasuryStub.CallCount);
    }

    [Fact]
    public async Task UnmatchedCommandType_ReturnsNull_AndNoLoaderInvoked()
    {
        var obligationStub = new ScriptedLoader();
        var treasuryStub = new ScriptedLoader();

        var composite = new CompositeAggregateStateLoader(new[]
        {
            new CompositeAggregateStateLoader.Route(ObligationStateLoader.Handles, obligationStub),
            new CompositeAggregateStateLoader.Route(TreasuryStateLoader.Handles, treasuryStub),
        });

        // A command that neither loader claims.
        var result = await composite.LoadSnapshotAsync(
            typeof(string), Guid.NewGuid());

        Assert.Null(result);
        Assert.Equal(0, obligationStub.CallCount);
        Assert.Equal(0, treasuryStub.CallCount);
    }

    [Fact]
    public async Task EmptyRouteList_ReturnsNull_ForAnyCommand()
    {
        var composite = new CompositeAggregateStateLoader(
            Array.Empty<CompositeAggregateStateLoader.Route>());

        var result = await composite.LoadSnapshotAsync(
            typeof(FulfilObligationCommand), Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task FirstMatch_Wins_OnPredicateOverlap()
    {
        // Two stubs both claim the same command type. The composite picks
        // the first in construction order — a deterministic fallback for
        // any future configuration drift.
        var first = new ScriptedLoader { Return = "first" };
        var second = new ScriptedLoader { Return = "second" };

        var composite = new CompositeAggregateStateLoader(new[]
        {
            new CompositeAggregateStateLoader.Route(
                _ => true /* matches everything */, first),
            new CompositeAggregateStateLoader.Route(
                _ => true /* would also match */, second),
        });

        var result = await composite.LoadSnapshotAsync(
            typeof(FulfilObligationCommand), Guid.NewGuid());

        Assert.Equal("first", result);
        Assert.Equal(1, first.CallCount);
        Assert.Equal(0, second.CallCount);
    }
}
