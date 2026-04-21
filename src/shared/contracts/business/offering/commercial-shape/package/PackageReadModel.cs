namespace Whycespace.Shared.Contracts.Business.Offering.CommercialShape.Package;

public sealed record PackageReadModel
{
    public Guid PackageId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public IReadOnlyList<PackageMemberReadModel> Members { get; init; } = Array.Empty<PackageMemberReadModel>();
}

public sealed record PackageMemberReadModel(string MemberKind, Guid MemberId);
