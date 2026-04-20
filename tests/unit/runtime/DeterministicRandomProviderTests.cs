using Whycespace.Runtime.Resilience;

namespace Whycespace.Tests.Unit.Runtime;

/// <summary>
/// R2.A.1 — pin the <see cref="DeterministicRandomProvider"/> contract:
/// same seed → same value; all outputs in documented bounds; no hidden
/// state. The retry executor and any future deterministic-jitter / rebalance
/// / probe-selection caller relies on these invariants.
/// </summary>
public sealed class DeterministicRandomProviderTests
{
    [Fact]
    public void NextDouble_Same_Seed_Returns_Same_Value()
    {
        var p = new DeterministicRandomProvider();
        var a = p.NextDouble("seed-1");
        var b = p.NextDouble("seed-1");
        Assert.Equal(a, b);
    }

    [Fact]
    public void NextDouble_Different_Seeds_Return_Different_Values()
    {
        var p = new DeterministicRandomProvider();
        var a = p.NextDouble("seed-alpha");
        var b = p.NextDouble("seed-beta");
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void NextDouble_Is_In_Zero_To_One_Range()
    {
        var p = new DeterministicRandomProvider();
        foreach (var seed in new[] { "s1", "s2", "s3", "the-quick-brown-fox", "a" })
        {
            var v = p.NextDouble(seed);
            Assert.InRange(v, 0.0, 1.0);
            Assert.NotEqual(1.0, v); // exclusive upper bound
        }
    }

    [Fact]
    public void NextInt_Same_Seed_Returns_Same_Value()
    {
        var p = new DeterministicRandomProvider();
        var a = p.NextInt("seed-1", 0, 100);
        var b = p.NextInt("seed-1", 0, 100);
        Assert.Equal(a, b);
    }

    [Fact]
    public void NextInt_Is_In_Range()
    {
        var p = new DeterministicRandomProvider();
        for (int i = 0; i < 50; i++)
        {
            var v = p.NextInt($"seed-{i}", 10, 50);
            Assert.InRange(v, 10, 49);
        }
    }

    [Fact]
    public void NextInt_Rejects_Invalid_Range()
    {
        var p = new DeterministicRandomProvider();
        Assert.Throws<ArgumentException>(() => p.NextInt("s", 10, 10));
        Assert.Throws<ArgumentException>(() => p.NextInt("s", 20, 10));
    }

    [Fact]
    public void NextLong_Same_Seed_Returns_Same_Value()
    {
        var p = new DeterministicRandomProvider();
        var a = p.NextLong("seed-1");
        var b = p.NextLong("seed-1");
        Assert.Equal(a, b);
    }

    [Fact]
    public void Empty_Seed_Throws()
    {
        var p = new DeterministicRandomProvider();
        Assert.Throws<ArgumentException>(() => p.NextDouble(""));
        Assert.Throws<ArgumentException>(() => p.NextInt("", 0, 10));
        Assert.Throws<ArgumentException>(() => p.NextLong(""));
    }
}
