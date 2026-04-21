using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Customer.IdentityAndProfile.Profile;

public sealed record CreateProfileCommand(
    Guid ProfileId,
    Guid CustomerId,
    string DisplayName) : IHasAggregateId
{
    public Guid AggregateId => ProfileId;
}

public sealed record RenameProfileCommand(
    Guid ProfileId,
    string DisplayName) : IHasAggregateId
{
    public Guid AggregateId => ProfileId;
}

public sealed record SetProfileDescriptorCommand(
    Guid ProfileId,
    string Key,
    string Value) : IHasAggregateId
{
    public Guid AggregateId => ProfileId;
}

public sealed record RemoveProfileDescriptorCommand(
    Guid ProfileId,
    string Key) : IHasAggregateId
{
    public Guid AggregateId => ProfileId;
}

public sealed record ActivateProfileCommand(Guid ProfileId) : IHasAggregateId
{
    public Guid AggregateId => ProfileId;
}

public sealed record ArchiveProfileCommand(Guid ProfileId) : IHasAggregateId
{
    public Guid AggregateId => ProfileId;
}
