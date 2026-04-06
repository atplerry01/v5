using Whycespace.Domain.SharedKernel.Regulatory.Jurisdiction;
using DomainEntity = Whycespace.Domain.SharedKernel.Primitives.Kernel.Entity;

namespace Whycespace.Domain.StructuralSystem.Structure.StructureRegistry;

public sealed class StructureNode : DomainEntity
{
    public StructureType Type { get; private set; } = default!;
    public StructureName Name { get; private set; } = default!;
    public JurisdictionCode JurisdictionCode { get; private set; } = default!;
    public StructureStatus Status { get; private set; } = StructureStatus.Active;

    private StructureNode() { }

    public static StructureNode Create(
        StructureId structureId,
        StructureType type,
        StructureName name,
        JurisdictionCode jurisdictionCode)
    {
        return new StructureNode
        {
            Id = structureId.Value,
            Type = type,
            Name = name,
            JurisdictionCode = jurisdictionCode,
            Status = StructureStatus.Active
        };
    }

    public void Deactivate()
    {
        Status = StructureStatus.Inactive;
    }

    public void Suspend()
    {
        Status = StructureStatus.Suspended;
    }

    public void Activate()
    {
        Status = StructureStatus.Active;
    }

    public void Reclassify(StructureType newType)
    {
        Type = newType;
    }
}
