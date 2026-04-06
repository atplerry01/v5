using System.Security.Cryptography;
using System.Text;
using Whyce.Shared.Contracts.Engine;

namespace Whyce.Engines.T0U.WhyceId;

public sealed class WhyceIdEngine
{
    public Task<WhyceIdResult> Handle(WhyceIdCommand command, IEngineContext context)
    {
        if (command.Token is null && command.UserId is null)
        {
            return Task.FromResult(new WhyceIdResult(
                Identity: new WhyceIdentity(
                    IdentityId: string.Empty,
                    IsAuthenticated: false,
                    IsVerified: false,
                    Roles: [],
                    TrustScore: 0),
                IsValid: false));
        }

        var identityId = command.UserId ?? HashToken(command.Token!);

        return Task.FromResult(new WhyceIdResult(
            Identity: new WhyceIdentity(
                IdentityId: identityId,
                IsAuthenticated: true,
                IsVerified: true,
                Roles: ["user"],
                TrustScore: 50),
            IsValid: true));
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexStringLower(bytes);
    }
}
