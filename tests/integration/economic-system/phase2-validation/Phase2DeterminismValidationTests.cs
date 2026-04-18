using Whycespace.Shared.Kernel.Domain;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Integration.EconomicSystem.Phase2Validation;

/// <summary>
/// Phase 2 validation / $9 Determinism gate.
///
/// Asserts the two determinism seams the economic-system relies on are
/// byte-stable:
///
///   D1 — TestIdGenerator produces the same Guid for the same seed across
///        repeated invocations and across fresh instances (mirrors the
///        production DeterministicIdGenerator contract).
///   D2 — TestIdGenerator produces distinct Guids for distinct seeds
///        (no accidental collisions in the SHA256-truncated space).
///   D3 — TestClock.Frozen is immutable across calls, so replay tests
///        always see the same UtcNow regardless of wall-clock drift.
///
/// These invariants gate all downstream Phase 2 correctness assertions
/// (identity, hashing, replay). If any fail, the suite halts before the
/// heavier concurrency and consistency tests run.
/// </summary>
public sealed class Phase2DeterminismValidationTests
{
    [Fact]
    public void D1_Same_Seed_Produces_Same_Id_Across_Instances()
    {
        IIdGenerator gen1 = new TestIdGenerator();
        IIdGenerator gen2 = new TestIdGenerator();

        var seed = "phase2:ledger:aggregate:001";

        var id1 = gen1.Generate(seed);
        var id2 = gen2.Generate(seed);
        var id1Again = gen1.Generate(seed);

        Assert.Equal(id1, id2);
        Assert.Equal(id1, id1Again);
    }

    [Fact]
    public void D2_Distinct_Seeds_Produce_Distinct_Ids()
    {
        IIdGenerator gen = new TestIdGenerator();
        var seen = new HashSet<Guid>();
        const int sampleCount = 10_000;

        for (var i = 0; i < sampleCount; i++)
        {
            var id = gen.Generate($"phase2:collision-probe:{i}");
            Assert.True(seen.Add(id), $"Collision at seed index {i}: {id}");
        }

        Assert.Equal(sampleCount, seen.Count);
    }

    [Fact]
    public void D3_TestClock_Is_Frozen()
    {
        IClock clock = new TestClock();

        var a = clock.UtcNow;
        Thread.Sleep(5);
        var b = clock.UtcNow;

        Assert.Equal(TestClock.Frozen, a);
        Assert.Equal(a, b);
    }
}
