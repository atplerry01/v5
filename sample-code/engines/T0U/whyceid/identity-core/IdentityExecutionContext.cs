using Whycespace.Runtime.Command;

namespace Whycespace.Engines.T0U.WhyceId;

/// <summary>
/// Typed accessor for identity data stored in CommandContext properties.
/// Provides a clean API for downstream middleware and engines to read identity claims
/// that were enriched by IdentityContextMiddleware.
/// </summary>
public sealed class IdentityExecutionContext
{
    private readonly CommandContext _context;

    public IdentityExecutionContext(CommandContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public string? IdentityId => _context.Get<string>(IdentityContextKeys.IdentityId);
    public string? IdentityType => _context.Get<string>(IdentityContextKeys.IdentityType);
    public string? IdentityStatus => _context.Get<string>(IdentityContextKeys.IdentityStatus);
    public string? SessionId => _context.Get<string>(IdentityContextKeys.SessionId);
    public string? DeviceId => _context.Get<string>(IdentityContextKeys.DeviceId);
    public string? TrustLevel => _context.Get<string>(IdentityContextKeys.TrustLevel);
    public string? AuthenticationMethod => _context.Get<string>(IdentityContextKeys.AuthenticationMethod);

    /// <summary>
    /// Whether the caller is a service identity. Stored as string "true"/"false" in context.
    /// </summary>
    public bool IsServiceIdentity
    {
        get
        {
            var value = _context.Get<string>(IdentityContextKeys.IsServiceIdentity);
            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(IdentityId);

    public static IdentityExecutionContext From(CommandContext context) => new(context);
}
