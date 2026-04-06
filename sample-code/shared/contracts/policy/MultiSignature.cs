using System.Security.Cryptography;
using System.Text;

namespace Whycespace.Shared.Contracts.Policy;

public sealed record MultiSignature
{
    public required IReadOnlyList<Guid> Signers { get; init; }
    public required string SignatureHash { get; init; }
    public required string CombinedSignature { get; init; }

    public static MultiSignature Create(IReadOnlyList<Guid> signers, IReadOnlyList<string> signatures)
    {
        if (signers.Count == 0) throw new ArgumentException("At least one signer required.");
        if (signers.Count != signatures.Count) throw new ArgumentException("Signer/signature count mismatch.");

        var combined = string.Join("|", signatures.OrderBy(s => s));
        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(combined)));

        return new MultiSignature
        {
            Signers = signers,
            SignatureHash = hash,
            CombinedSignature = combined
        };
    }

    public bool ContainsSigner(Guid signerId) => Signers.Contains(signerId);
}
