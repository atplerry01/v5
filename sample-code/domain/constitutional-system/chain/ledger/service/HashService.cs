using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Whycespace.Domain.ConstitutionalSystem.Chain.Ledger;

public static class HashService
{
    private static readonly JsonSerializerOptions DeterministicOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public static string ComputeSHA256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }

    public static string SerializePayload(object payload)
    {
        return JsonSerializer.Serialize(payload, DeterministicOptions);
    }
}
