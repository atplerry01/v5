using System.Security.Cryptography;
using System.Text;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Composition.Core;

internal sealed class DeterministicIdGenerator : IIdGenerator
{
    public Guid Generate(string seed)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
        return new Guid(hash.AsSpan(0, 16));
    }
}
