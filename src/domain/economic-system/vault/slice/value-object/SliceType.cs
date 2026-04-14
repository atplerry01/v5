namespace Whycespace.Domain.EconomicSystem.Vault.Slice;

/// <summary>
/// Doctrine-locked four-slice vault model.
///   Slice1 — liquidity gateway (sole entry/exit point)
///   Slice2 — investment staging (fed from Slice1)
///   Slice3 — reserved for future doctrine extension
///   Slice4 — reserved for future doctrine extension
/// </summary>
public enum SliceType
{
    Slice1 = 1,
    Slice2 = 2,
    Slice3 = 3,
    Slice4 = 4
}
