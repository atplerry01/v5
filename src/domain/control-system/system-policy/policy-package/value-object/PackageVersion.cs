namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyPackage;

public readonly record struct PackageVersion
{
    public int Major { get; }
    public int Minor { get; }

    public PackageVersion(int major, int minor)
    {
        if (major < 1)
            throw PolicyPackageErrors.PackageVersionMajorMustBePositive();

        if (minor < 0)
            throw PolicyPackageErrors.PackageVersionMinorMustBeNonNegative();

        Major = major;
        Minor = minor;
    }

    public override string ToString() => $"{Major}.{Minor}";
}
