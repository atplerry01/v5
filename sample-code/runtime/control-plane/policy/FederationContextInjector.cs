using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Runtime.ControlPlane.Policy;

/// <summary>
/// Injects federation graph context before policy evaluation in the pipeline.
/// Resolves the federation graph from the repository and makes it available
/// for federation-aware evaluation.
/// RUNTIME ONLY — no business logic, orchestration only.
/// </summary>
public sealed class FederationContextInjector
{
    private readonly IPolicyFederationRepository _federationRepository;

    public FederationContextInjector(IPolicyFederationRepository federationRepository)
    {
        ArgumentNullException.ThrowIfNull(federationRepository);
        _federationRepository = federationRepository;
    }

    public async Task<FederationGraphDto?> ResolveFederationGraphAsync(
        string? graphHash,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(graphHash))
            return null;

        return await _federationRepository.GetGraphByHashAsync(graphHash, ct);
    }
}
