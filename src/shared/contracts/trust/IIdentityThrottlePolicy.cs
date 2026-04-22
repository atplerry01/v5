namespace Whycespace.Shared.Contracts.Trust;

/// <summary>
/// Guards identity operations against brute-force and enumeration attacks.
/// Implementations track attempt counts per throttle key (e.g. IP, actor, email)
/// within a sliding window and report whether the caller is currently throttled.
/// </summary>
public interface IIdentityThrottlePolicy
{
    /// <summary>
    /// Returns true if the caller identified by <paramref name="key"/> has exceeded
    /// the allowed attempt rate and should be refused. Implementations must never
    /// throw — return false on internal error to favour availability over lockout.
    /// </summary>
    Task<bool> IsThrottledAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a failed attempt for <paramref name="key"/>. Called after every
    /// failed identity operation so the window advances correctly.
    /// </summary>
    Task RecordFailedAttemptAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets the attempt counter for <paramref name="key"/> on successful
    /// authentication or registration completion.
    /// </summary>
    Task ResetAsync(string key, CancellationToken cancellationToken = default);
}
