namespace Whycespace.Shared.Kernel.Determinism;

/// <summary>
/// HSID v2.1 location segment (LLLL). Four uppercase chars naming the
/// physical / regulatory site (e.g. UKED, NGLG) OR a hex-derived 4-char
/// prefix when no authoritative mnemonic is supplied (e.g. EAF8 from a
/// SHA256(tenantId) prefix). Carried as a value object so the engine can
/// validate width without per-call string parsing. Allowed alphabet matches
/// the canonical format regex: [A-Z0-9]{4}.
/// </summary>
public readonly record struct LocationCode(string Value)
{
    public override string ToString() => Value;
}
