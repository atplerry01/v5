using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyPackage;

public static class PolicyPackageErrors
{
    public static DomainException PackageIdMustNotBeEmpty() =>
        new DomainInvariantViolationException("PolicyPackageId must not be null or empty.");

    public static DomainException PackageIdMustBe64HexChars(string value) =>
        new DomainInvariantViolationException(
            $"PolicyPackageId must be exactly 64 lowercase hex characters. Got: '{value}'.");

    public static DomainException PackageNameMustNotBeEmpty() =>
        new DomainInvariantViolationException("PolicyPackage name must not be null or empty.");

    public static DomainException PackageMustContainAtLeastOnePolicy() =>
        new DomainInvariantViolationException("PolicyPackage must contain at least one policy definition.");

    public static DomainException PackageAlreadyDeployed() =>
        new DomainInvariantViolationException("PolicyPackage is already deployed.");

    public static DomainException PackageAlreadyRetired() =>
        new DomainInvariantViolationException("PolicyPackage is already retired.");

    public static DomainException PackageMustBeAssembledBeforeDeployment() =>
        new DomainInvariantViolationException("PolicyPackage must be in Assembled status before it can be deployed.");

    public static DomainException PackageVersionMajorMustBePositive() =>
        new DomainInvariantViolationException("PackageVersion major must be greater than zero.");

    public static DomainException PackageVersionMinorMustBeNonNegative() =>
        new DomainInvariantViolationException("PackageVersion minor must be zero or greater.");
}
