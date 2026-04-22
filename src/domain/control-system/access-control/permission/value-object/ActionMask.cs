namespace Whycespace.Domain.ControlSystem.AccessControl.Permission;

[Flags]
public enum ActionMask
{
    None = 0,
    Read = 1,
    Write = 2,
    Admin = 4,
    All = Read | Write | Admin
}
