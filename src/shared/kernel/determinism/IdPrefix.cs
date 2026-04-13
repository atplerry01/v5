namespace Whycespace.Shared.Kernel.Determinism;

/// <summary>
/// HSID v2.1 prefix segment (PPP). Three uppercase characters.
/// Identifies the kind of artifact the ID names.
/// </summary>
public enum IdPrefix
{
    ACC,
    EVT,
    CMD,
    WF,
    POL,
    BLK,
    IDN,
    TXN,
    SPV,
    PRJ
}
