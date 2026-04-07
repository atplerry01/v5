using System.Security.Cryptography;
using System.Text;
using Whyce.Shared.Kernel.Domain;

namespace Whycespace.Tests.Shared;

/// <summary>
/// SHA256-seeded deterministic id generator for tests. Same seed → same Guid.
/// Mirrors the production DeterministicIdGenerator in src/platform/host/Program.cs.
/// </summary>
public sealed class TestIdGenerator : IIdGenerator
{
    public Guid Generate(string seed)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
        return new Guid(hash.AsSpan(0, 16));
    }
}
