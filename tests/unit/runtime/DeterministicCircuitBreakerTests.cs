using Whycespace.Runtime.Resilience;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Tests.Unit.Runtime;

/// <summary>
/// R2.A.D.1 / R-CIRCUIT-BREAKER-01 — pin every state transition of
/// <see cref="DeterministicCircuitBreaker"/>. Fake clock drives the
/// window semantics deterministically without <see cref="Task.Delay"/>
/// so the tests are fast and replay-stable.
/// </summary>
public sealed class DeterministicCircuitBreakerTests
{
    private sealed class FakeClock : IClock
    {
        public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.Parse("2026-04-19T00:00:00Z");
        public void Advance(TimeSpan by) => UtcNow = UtcNow.Add(by);
    }

    private sealed class BoomException : Exception
    {
        public BoomException(string message = "dependency failed") : base(message) { }
    }

    private static DeterministicCircuitBreaker NewBreaker(
        FakeClock clock,
        int failureThreshold = 3,
        int windowSeconds = 30,
        string name = "test-breaker") =>
        new(new CircuitBreakerOptions
            {
                Name = name,
                FailureThreshold = failureThreshold,
                WindowSeconds = windowSeconds
            },
            clock);

    // ─────────────────────────────────────────────────────────────────────
    // Closed path
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Closed_Breaker_Runs_Operation_And_Returns_Value()
    {
        var breaker = NewBreaker(new FakeClock());
        var result = await breaker.ExecuteAsync<int>((_) => Task.FromResult(42));
        Assert.Equal(42, result);
        Assert.Equal(CircuitBreakerState.Closed, breaker.State);
    }

    [Fact]
    public async Task Success_Resets_Consecutive_Failure_Counter()
    {
        var breaker = NewBreaker(new FakeClock(), failureThreshold: 3);

        // Two failures, then a success — counter should reset so the breaker stays Closed.
        for (int i = 0; i < 2; i++)
        {
            try { await breaker.ExecuteAsync<int>((_) => throw new BoomException()); }
            catch (BoomException) { }
        }
        await breaker.ExecuteAsync<int>((_) => Task.FromResult(1));

        // Two more failures after the success — still Closed (counter was reset).
        for (int i = 0; i < 2; i++)
        {
            try { await breaker.ExecuteAsync<int>((_) => throw new BoomException()); }
            catch (BoomException) { }
        }
        Assert.Equal(CircuitBreakerState.Closed, breaker.State);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Closed → Open transition
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Threshold_Consecutive_Failures_Opens_The_Breaker()
    {
        var breaker = NewBreaker(new FakeClock(), failureThreshold: 3);

        for (int i = 0; i < 3; i++)
        {
            try { await breaker.ExecuteAsync<int>((_) => throw new BoomException()); }
            catch (BoomException) { }
        }

        Assert.Equal(CircuitBreakerState.Open, breaker.State);
    }

    [Fact]
    public async Task Open_Breaker_Throws_Without_Running_Operation()
    {
        var breaker = NewBreaker(new FakeClock(), failureThreshold: 2);

        for (int i = 0; i < 2; i++)
        {
            try { await breaker.ExecuteAsync<int>((_) => throw new BoomException()); }
            catch (BoomException) { }
        }

        int innerCalls = 0;
        var ex = await Assert.ThrowsAsync<CircuitBreakerOpenException>(() =>
            breaker.ExecuteAsync<int>((_) =>
            {
                innerCalls++;
                return Task.FromResult(1);
            }));

        Assert.Equal(0, innerCalls); // operation NOT executed
        Assert.Equal("test-breaker", ex.BreakerName);
        Assert.True(ex.RetryAfterSeconds > 0);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Open → HalfOpen and trial outcomes
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Window_Elapsed_Transitions_State_To_HalfOpen()
    {
        var clock = new FakeClock();
        var breaker = NewBreaker(clock, failureThreshold: 2, windowSeconds: 30);

        for (int i = 0; i < 2; i++)
        {
            try { await breaker.ExecuteAsync<int>((_) => throw new BoomException()); }
            catch (BoomException) { }
        }
        Assert.Equal(CircuitBreakerState.Open, breaker.State);

        // Advance past the window — State getter reports HalfOpen without
        // consuming the trial slot.
        clock.Advance(TimeSpan.FromSeconds(31));
        Assert.Equal(CircuitBreakerState.HalfOpen, breaker.State);
    }

    [Fact]
    public async Task HalfOpen_Trial_Success_Transitions_To_Closed()
    {
        var clock = new FakeClock();
        var breaker = NewBreaker(clock, failureThreshold: 2, windowSeconds: 30);

        for (int i = 0; i < 2; i++)
        {
            try { await breaker.ExecuteAsync<int>((_) => throw new BoomException()); }
            catch (BoomException) { }
        }
        clock.Advance(TimeSpan.FromSeconds(31));

        var result = await breaker.ExecuteAsync<int>((_) => Task.FromResult(99));

        Assert.Equal(99, result);
        Assert.Equal(CircuitBreakerState.Closed, breaker.State);
    }

    [Fact]
    public async Task HalfOpen_Trial_Failure_Re_Opens_The_Breaker()
    {
        var clock = new FakeClock();
        var breaker = NewBreaker(clock, failureThreshold: 2, windowSeconds: 30);

        for (int i = 0; i < 2; i++)
        {
            try { await breaker.ExecuteAsync<int>((_) => throw new BoomException()); }
            catch (BoomException) { }
        }
        clock.Advance(TimeSpan.FromSeconds(31));

        try { await breaker.ExecuteAsync<int>((_) => throw new BoomException("still broken")); }
        catch (BoomException) { }

        // Back to Open, fresh window starts now.
        Assert.Equal(CircuitBreakerState.Open, breaker.State);

        // A subsequent call immediately throws breaker-open (within new window).
        await Assert.ThrowsAsync<CircuitBreakerOpenException>(() =>
            breaker.ExecuteAsync<int>((_) => Task.FromResult(1)));
    }

    // ─────────────────────────────────────────────────────────────────────
    // State getter is side-effect-free
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task State_Getter_Does_Not_Consume_HalfOpen_Trial_Slot()
    {
        var clock = new FakeClock();
        var breaker = NewBreaker(clock, failureThreshold: 2, windowSeconds: 30);

        for (int i = 0; i < 2; i++)
        {
            try { await breaker.ExecuteAsync<int>((_) => throw new BoomException()); }
            catch (BoomException) { }
        }
        clock.Advance(TimeSpan.FromSeconds(31));

        // Poll the State getter many times — must not transition state or consume the trial.
        for (int i = 0; i < 100; i++)
            Assert.Equal(CircuitBreakerState.HalfOpen, breaker.State);

        // The NEXT ExecuteAsync is still the trial.
        var result = await breaker.ExecuteAsync<int>((_) => Task.FromResult(7));
        Assert.Equal(7, result);
        Assert.Equal(CircuitBreakerState.Closed, breaker.State);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Cancellation propagation
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Cancellation_Is_Propagated_And_NOT_Counted_As_Failure()
    {
        var breaker = NewBreaker(new FakeClock(), failureThreshold: 3);

        for (int i = 0; i < 5; i++)
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                breaker.ExecuteAsync<int>(async (ct) =>
                {
                    await Task.Delay(Timeout.Infinite, ct); // will throw OCE / TaskCanceledException
                    return 1;
                }, cts.Token));
        }

        // Five cancellations should NOT trip the breaker — they aren't failures.
        Assert.Equal(CircuitBreakerState.Closed, breaker.State);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Concurrent contention — Open-state throws are safe under N callers
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task N_Concurrent_Calls_During_Open_State_All_Throw_Without_Running_Operation()
    {
        var clock = new FakeClock();
        var breaker = NewBreaker(clock, failureThreshold: 2, windowSeconds: 30);

        for (int i = 0; i < 2; i++)
        {
            try { await breaker.ExecuteAsync<int>((_) => throw new BoomException()); }
            catch (BoomException) { }
        }
        Assert.Equal(CircuitBreakerState.Open, breaker.State);

        int innerCalls = 0;
        const int racers = 50;
        using var barrier = new Barrier(racers);

        var tasks = Enumerable.Range(0, racers).Select(_ => Task.Run(async () =>
        {
            barrier.SignalAndWait();
            try
            {
                await breaker.ExecuteAsync<int>((_) =>
                {
                    Interlocked.Increment(ref innerCalls);
                    return Task.FromResult(1);
                });
                return false; // no throw
            }
            catch (CircuitBreakerOpenException) { return true; }
        })).ToArray();

        var results = await Task.WhenAll(tasks);

        Assert.Equal(racers, results.Count(r => r)); // every caller threw
        Assert.Equal(0, innerCalls);                  // operation NEVER executed
    }

    // ─────────────────────────────────────────────────────────────────────
    // Replay determinism — same clock + same outcomes → identical state
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Two_Breakers_With_Same_Clock_And_Outcomes_Reach_Identical_State()
    {
        var clock1 = new FakeClock();
        var clock2 = new FakeClock();
        var b1 = NewBreaker(clock1, failureThreshold: 3, windowSeconds: 30);
        var b2 = NewBreaker(clock2, failureThreshold: 3, windowSeconds: 30);

        async Task DriveAsync(DeterministicCircuitBreaker b, FakeClock c)
        {
            for (int i = 0; i < 3; i++)
            {
                try { await b.ExecuteAsync<int>((_) => throw new BoomException()); }
                catch (BoomException) { }
            }
            c.Advance(TimeSpan.FromSeconds(31));
            await b.ExecuteAsync<int>((_) => Task.FromResult(1)); // trial success
        }

        await DriveAsync(b1, clock1);
        await DriveAsync(b2, clock2);

        Assert.Equal(b1.State, b2.State);
        Assert.Equal(CircuitBreakerState.Closed, b1.State);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Input validation
    // ─────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Options_Requires_NonBlank_Name(string? name)
    {
        Assert.Throws<ArgumentException>(() =>
            new DeterministicCircuitBreaker(
                new CircuitBreakerOptions { Name = name! },
                new FakeClock()));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Options_Requires_Positive_FailureThreshold(int threshold)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new DeterministicCircuitBreaker(
                new CircuitBreakerOptions { Name = "x", FailureThreshold = threshold },
                new FakeClock()));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Options_Requires_Positive_WindowSeconds(int window)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new DeterministicCircuitBreaker(
                new CircuitBreakerOptions { Name = "x", WindowSeconds = window },
                new FakeClock()));
    }
}
