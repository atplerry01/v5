using Whycespace.Runtime.Security;

namespace Whycespace.Tests.Unit.TrustSystem.Security;

/// <summary>
/// 2.8.21 — Certification tests for InMemoryIdentityThrottlePolicy.
/// Verifies sliding-window rate limiting for identity operations.
/// </summary>
public sealed class InMemoryIdentityThrottlePolicyTests
{
    [Fact]
    public async Task IsThrottled_BeforeMaxAttempts_ReturnsFalse()
    {
        var policy = new InMemoryIdentityThrottlePolicy(maxAttempts: 3);

        await policy.RecordFailedAttemptAsync("user:alice");
        await policy.RecordFailedAttemptAsync("user:alice");

        var throttled = await policy.IsThrottledAsync("user:alice");

        Assert.False(throttled);
    }

    [Fact]
    public async Task IsThrottled_AfterMaxAttempts_ReturnsTrue()
    {
        var policy = new InMemoryIdentityThrottlePolicy(maxAttempts: 3);

        await policy.RecordFailedAttemptAsync("user:bob");
        await policy.RecordFailedAttemptAsync("user:bob");
        await policy.RecordFailedAttemptAsync("user:bob");

        var throttled = await policy.IsThrottledAsync("user:bob");

        Assert.True(throttled);
    }

    [Fact]
    public async Task IsThrottled_ForUnknownKey_ReturnsFalse()
    {
        var policy = new InMemoryIdentityThrottlePolicy(maxAttempts: 3);

        var throttled = await policy.IsThrottledAsync("user:unknown");

        Assert.False(throttled);
    }

    [Fact]
    public async Task Reset_ClearsAttempts_AllowsNewAttempts()
    {
        var policy = new InMemoryIdentityThrottlePolicy(maxAttempts: 2);

        await policy.RecordFailedAttemptAsync("user:carol");
        await policy.RecordFailedAttemptAsync("user:carol");
        Assert.True(await policy.IsThrottledAsync("user:carol"));

        await policy.ResetAsync("user:carol");

        Assert.False(await policy.IsThrottledAsync("user:carol"));
    }

    [Fact]
    public async Task IsThrottled_DifferentKeys_TrackSeparately()
    {
        var policy = new InMemoryIdentityThrottlePolicy(maxAttempts: 2);

        await policy.RecordFailedAttemptAsync("user:dave");
        await policy.RecordFailedAttemptAsync("user:dave");

        Assert.True(await policy.IsThrottledAsync("user:dave"));
        Assert.False(await policy.IsThrottledAsync("user:eve"));
    }

    [Fact]
    public async Task IsThrottled_WindowExpiry_PrunesOldAttempts()
    {
        var shortWindow = TimeSpan.FromMilliseconds(50);
        var policy = new InMemoryIdentityThrottlePolicy(maxAttempts: 2, window: shortWindow);

        await policy.RecordFailedAttemptAsync("user:frank");
        await policy.RecordFailedAttemptAsync("user:frank");
        Assert.True(await policy.IsThrottledAsync("user:frank"));

        await Task.Delay(100);

        Assert.False(await policy.IsThrottledAsync("user:frank"));
    }

    [Fact]
    public async Task RecordFailedAttempt_IsIdempotentForSameKey()
    {
        var policy = new InMemoryIdentityThrottlePolicy(maxAttempts: 10);

        for (var i = 0; i < 5; i++)
            await policy.RecordFailedAttemptAsync("user:grace");

        Assert.False(await policy.IsThrottledAsync("user:grace"));
    }
}
