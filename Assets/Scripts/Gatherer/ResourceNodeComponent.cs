using Unity.Entities;
using Unity.Mathematics;

public struct ResourceNodeComponent : IComponentData
{
    public int ResourceId;
    public CubeIndex CubeIndex;
}
