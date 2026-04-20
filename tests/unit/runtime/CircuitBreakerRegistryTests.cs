using Whycespace.Runtime.Resilience;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Tests.Unit.Runtime;

/// <summary>
/// R2.A.D.4 / R-BREAKER-REGISTRY-01 — contract tests for
/// <see cref="CircuitBreakerRegistry"/>. Covers named lookup,
/// TryGet null-tolerance, enumeration determinism (sorted by Name),
/// duplicate-name rejection, and input validation.
/// </summary>
public sealed class CircuitBreakerRegistryTests
{
    private sealed class FakeClock : IClock
    {
        public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.Parse("2026-04-19T00:00:00Z");
    }

    private static ICircuitBreaker NewBreaker(string name) =>
        new DeterministicCircuitBreaker(
            new CircuitBreakerOptions { Name = name, FailureThreshold = 3, WindowSeconds = 30 },
            new FakeClock());

    [Fact]
    public void Get_Returns_Registered_Instance_By_Name()
    {
        var a = NewBreaker("alpha");
        var b = NewBreaker("beta");
        var reg = new CircuitBreakerRegistry(new[] { a, b });

        Assert.Same(a, reg.Get("alpha"));
        Assert.Same(b, reg.Get("beta"));
    }

    [Fact]
    public void Get_Throws_KeyNotFoundException_For_Unknown_Name()
    {
        var reg = new CircuitBreakerRegistry(new[] { NewBreaker("alpha") });
        var ex = Assert.Throws<KeyNotFoundException>(() => reg.Get("gamma"));
        Assert.Contains("gamma", ex.Message);
        Assert.Contains("alpha", ex.Message); // Lists what IS registered.
    }

    [Fact]
    public void TryGet_Returns_Null_For_Unknown_Name_Without_Throwing()
    {
        var reg = new CircuitBreakerRegistry(new[] { NewBreaker("alpha") });
        Assert.Null(reg.TryGet("gamma"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryGet_Is_Null_Tolerant_On_Blank_Input(string? name)
    {
        var reg = new CircuitBreakerRegistry(new[] { NewBreaker("alpha") });
        Assert.Null(reg.TryGet(name!));
    }

    [Fact]
    public void GetAll_Returns_All_Breakers_Sorted_By_Name()
    {
        var zeta = NewBreaker("zeta");
        var alpha = NewBreaker("alpha");
        var mu = NewBreaker("mu");

        var reg = new CircuitBreakerRegistry(new[] { zeta, alpha, mu });

        var ordered = reg.GetAll().ToList();
        Assert.Equal(3, ordered.Count);
        Assert.Equal("alpha", ordered[0].Name);
        Assert.Equal("mu", ordered[1].Name);
        Assert.Equal("zeta", ordered[2].Name);
    }

    [Fact]
    public void Duplicate_Name_Throws_At_Construction()
    {
        var a1 = NewBreaker("shared");
        var a2 = NewBreaker("shared");

        var ex = Assert.Throws<ArgumentException>(() =>
            new CircuitBreakerRegistry(new[] { a1, a2 }));
        Assert.Contains("shared", ex.Message);
    }

    [Fact]
    public void Null_Or_Blank_Breaker_Name_Throws_At_Construction()
    {
        // A fake ICircuitBreaker with a blank name — catches composition bugs where
        // a breaker is registered without a proper Name.
        var blank = new FakeCircuitBreaker(name: "   ");
        Assert.Throws<ArgumentException>(() =>
            new CircuitBreakerRegistry(new ICircuitBreaker[] { blank }));
    }

    [Fact]
    public void Empty_Registry_GetAll_Returns_Empty()
    {
        var reg = new CircuitBreakerRegistry(Array.Empty<ICircuitBreaker>());
        Assert.Empty(reg.GetAll());
    }

    // Test double for the blank-name test — real DeterministicCircuitBreaker
    // rejects blank Name at construction.
    private sealed class FakeCircuitBreaker : ICircuitBreaker
    {
        public FakeCircuitBreaker(string name) { Name = name; }
        public string Name { get; }
        public CircuitBreakerState State => CircuitBreakerState.Closed;
        public Task<T> ExecuteAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken = default) =>
            operation(cancellationToken);
    }
}
