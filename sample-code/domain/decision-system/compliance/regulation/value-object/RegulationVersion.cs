namespace Whycespace.Domain.DecisionSystem.Compliance.Regulation;

public sealed record RegulationVersion(int Major, int Minor, int Patch) : IComparable<RegulationVersion>
{
    public int CompareTo(RegulationVersion? other)
    {
        if (other is null) return 1;

        var majorCompare = Major.CompareTo(other.Major);
        if (majorCompare != 0) return majorCompare;

        var minorCompare = Minor.CompareTo(other.Minor);
        if (minorCompare != 0) return minorCompare;

        return Patch.CompareTo(other.Patch);
    }

    public override string ToString() => $"{Major}.{Minor}.{Patch}";
}
