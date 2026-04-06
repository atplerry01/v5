using System.Security.Cryptography;
using System.Text;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Shared.Primitives.Id;

public sealed class WhyceIdGenerator : IIdGenerator
{
    private readonly IClock _clock;

    public WhyceIdGenerator(IClock clock)
    {
        _clock = clock;
    }

    public string Generate(IdGenerationContext context)
    {
        if (context.DeterministicKey is null)
            throw new InvalidOperationException(
                "DeterministicKey is required. Non-deterministic ID generation is not permitted. " +
                "Provide a stable seed via IdGenerationContext.DeterministicKey.");

        var timestamp = context.Timestamp.ToString("yyyyMMddHHmmssfff");
        var suffix = CreateDeterministicSuffix(context.DeterministicKey);

        return $"{context.Domain}-{context.Type}-{context.Jurisdiction}-{timestamp}-{suffix}";
    }

    private static string CreateDeterministicSuffix(string input)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash)[..6];
    }
}
