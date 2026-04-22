using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Access.Role;

public readonly record struct RoleDescriptor
{
    public string RoleName { get; }
    public string RoleScope { get; }

    public RoleDescriptor(string roleName, string roleScope)
    {
        Guard.Against(string.IsNullOrWhiteSpace(roleName), "RoleDescriptor.RoleName must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(roleScope), "RoleDescriptor.RoleScope must not be empty.");

        RoleName = roleName.Trim();
        RoleScope = roleScope.Trim();
    }
}
