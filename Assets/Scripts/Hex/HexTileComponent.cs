using UnityEngine;
using System.Collections;
using Unity.Entities;

public struct CubeIndex
{
    public int x;
    public int y;
    public int z;

    public CubeIndex(int x, int y, int z)
    {
        this.x = x; this.y = y; this.z = z;
    }

    public CubeIndex(int x, int z)
    {
        this.x = x; this.z = z; this.y = -x - z;
    }

    public static CubeIndex operator +(CubeIndex one, CubeIndex two)
    {
        return new CubeIndex(one.x + two.x, one.y + two.y, one.z + two.z);
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;
        CubeIndex o = (CubeIndex)obj;
        if ((System.Object)o == null)
            return false;
        return ((x == o.x) && (y == o.y) && (z == o.z));
    }

    public override int GetHashCode()
    {
        return (x.GetHashCode() ^ (y.GetHashCode() + (int)(Mathf.Pow(2, 32) / (1 + Mathf.Sqrt(5)) / 2) + (x.GetHashCode() << 6) + (x.GetHashCode() >> 2)));
    }

    public override string ToString()
    {
        return string.Format("[" + x + "," + y + "," + z + "]");
    }
}

public struct HexTileComponent : IComponentData
{
    public CubeIndex index;
}

public struct HexTileHightlightComponent : IComponentData
{
    //public CubeIndex index;
}