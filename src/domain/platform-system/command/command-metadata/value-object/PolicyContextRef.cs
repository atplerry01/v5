namespace Whycespace.Domain.PlatformSystem.Command.CommandMetadata;

public readonly record struct PolicyContextRef
{
    public string? PolicyId { get; }
    public string? PolicyVersion { get; }

    public PolicyContextRef(string? policyId, string? policyVersion)
    {
        PolicyId = policyId;
        PolicyVersion = policyVersion;
    }

    public bool IsPresent => !string.IsNullOrWhiteSpace(PolicyId);
}
