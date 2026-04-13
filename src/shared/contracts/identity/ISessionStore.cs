namespace Whycespace.Shared.Contracts.Identity;

/// <summary>
/// Contract for session persistence.
/// Sessions are deterministically generated but may need external validation.
/// </summary>
public interface ISessionStore
{
    Task<bool> ValidateAsync(string sessionId, string identityId);
    Task StoreAsync(string sessionId, string identityId, string? deviceId);
    Task InvalidateAsync(string sessionId);
}
