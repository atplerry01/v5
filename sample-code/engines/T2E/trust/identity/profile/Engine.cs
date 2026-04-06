namespace Whycespace.Engines.T2E.Trust.Identity.Profile;

public class ProfileEngine
{
    private readonly ProfilePolicyAdapter _policy;

    public ProfileEngine(ProfilePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ProfileResult> ExecuteAsync(ProfileCommand command)
    {
        await _policy.EnforceAsync(command);

        // Domain logic — emit events only (no persistence)

        return new ProfileResult(true, "Executed");
    }
}
