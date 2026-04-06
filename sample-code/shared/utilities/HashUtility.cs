using System.Security.Cryptography;
using System.Text;

namespace Whycespace.Shared.Utilities;

public static class HashUtility
{
    public static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }

    public static string ComputeSha256(byte[] input)
    {
        var bytes = SHA256.HashData(input);
        return Convert.ToHexStringLower(bytes);
    }
}
