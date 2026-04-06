namespace Whycespace.Engines.T0U.WhycePolicy.Safeguard;

public interface IPolicySafeguardEngine
{
    Task<SafeguardResult> ValidateSafeguardsAsync(
        Guid policyId,
        CancellationToken cancellationToken = default);
}

public sealed record SafeguardResult(
    bool IsCompliant,
    IReadOnlyList<string> FailedSafeguards);
