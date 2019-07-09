using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

public enum HexOrientation
{
    Pointy,
    Flat
}

public enum HexDirection
{
    NE, E, SE, SW, W, NW
}

public struct Point2D
{
    public int x;
    public int y;

    public static Point2D operator *(Point2D p, int k)
    {
        return new Point2D { x = p.x * k, y = p.y * k };
    }

    public static Point2D operator -(Point2D p1, Point2D p2)
    {
        return new Point2D { x = p1.x - p2.x, y = p1.y - p2.y };
    }

    public static Point2D operator +(Point2D p1, Point2D p2)
    {
        return new Point2D { x = p1.x + p2.x, y = p1.y + p2.y };
    }
}

public static class HexDirectionExtensions
{
    public static Point2D[] Dirs =
    {
        new Point2D { x = 0, y = 1 },
        new Point2D { x = 1, y = 0 },
        new Point2D { x = 1, y = -1 },
        new Point2D { x = 0, y = -1 },
        new Point2D { x = -1,y = 0 },
        new Point2D { x = -1,y = 1 },
    };

    public static HexDirection Opposite(this HexDirection direction)
    {
        return (int)direction < 3 ? (direction + 3) : (direction - 3);
    }

    public static HexDirection Previous(this HexDirection direction)
    {
        return direction == HexDirection.NE ? HexDirection.NW : (direction - 1);
    }

    public static HexDirection Next(this HexDirection direction)
    {
        return direction == HexDirection.NW ? HexDirection.NE : (direction + 1);
    }
}

public static class HexUtils
{
    public static Vector3 Corner(Vector3 origin, float radius, int corner, HexOrientation orientation)
    {
        float angle = 60 * corner;
        if (orientation == HexOrientation.Pointy)
            angle += 30;
        angle *= Mathf.PI / 180;
        return new Vector3(origin.x + radius * Mathf.Cos(angle), 0.0f, origin.z + radius * Mathf.Sin(angle));
    }

    public static void GetHexMesh(float radius, HexOrientation orientation, ref Mesh mesh)
    {
        mesh = new Mesh();

        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        for (int i = 0; i < 6; i++)
            verts.Add(Corner(Vector3.zero, radius, i, orientation));

        tris.Add(0);
        tris.Add(2);
        tris.Add(1);

        tris.Add(0);
        tris.Add(5);
        tris.Add(2);

        tris.Add(2);
        tris.Add(5);
        tris.Add(3);

        tris.Add(3);
        tris.Add(5);
        tris.Add(4);

        //UVs are wrong, I need to find an equation for calucalting them
        uvs.Add(new Vector2(0.5f, 1f));
        uvs.Add(new Vector2(1, 0.75f));
        uvs.Add(new Vector2(1, 0.25f));
        uvs.Add(new Vector2(0.5f, 0));
        uvs.Add(new Vector2(0, 0.25f));
        uvs.Add(new Vector2(0, 0.75f));

        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.name = "Hexagonal Plane";

        mesh.RecalculateNormals();
    }

    public static float3 ToWorldPos(this CubeIndex index, float outerRadius)
    {
        float3 pos = new float3(0, 0, 0);
        pos.x = outerRadius * 3.0f / 2.0f * index.x;
        pos.z = outerRadius * Mathf.Sqrt(3.0f) * (index.y + index.x / 2.0f);
        return pos;
    }

    public const float outerToInner = 0.866025404f;
    public const float innerToOuter = 1f / outerToInner;
    public static CubeIndex FromPosition(Vector3 position, float outerRadius)
    {
        float innerRadius = outerRadius * outerToInner;

        float column = position.z / (innerRadius * 2f);
        float row = -column;

        float offset = position.x / (outerRadius * 3f);
        column -= offset;
        row -= offset;

        int iY = Mathf.RoundToInt(column);
        int iZ = Mathf.RoundToInt(row);
        int iX = Mathf.RoundToInt(-column - row);

        if (iX + iY + iZ != 0)
        {
            float dX = Mathf.Abs(column - iX);
            float dY = Mathf.Abs(row - iY);
            float dZ = Mathf.Abs(-column - row - iZ);

            if (dX > dY && dX > dZ)
            {
                iX = -iY - iZ;
            }
            else if (dZ > dY)
            {
                iZ = -iX - iY;
            }
        }

        return new CubeIndex(iX, iZ);
    }

    //public static CubeIndex FromPosition(Vector3 position, float hexRadius)
    //{
    //    position /= hexRadius;
    //    hexRadius = 1;
    //    int column;
    //    int row;

    //    // Find out which major row and column we are on:
    //    row = (int)(position.z / 0.87f);
    //    column = (int)(position.x / (hexRadius + hexRadius / 2));

    //    // Compute the offset into these row and column:
    //    float dz = position.z - (float)row * 0.87f;
    //    float dx = position.x - (float)column * (hexRadius + hexRadius / 2);

    //    // Are we on the left of the hexagon edge, or on the right?
    //    if (((row ^ column) & 1) == 0)
    //    {
    //        dz = 0.87f - dz;
    //    }

    //    int right = dz * (hexRadius - hexRadius / 2) < 0.87f * (dx - hexRadius / 2) ? 1 : 0;

    //    // Now we have all the information we need, just fine-tune row and column.
    //    row += (column ^ row ^ right) & 1;
    //    column += right;

    //    int iX = Mathf.RoundToInt(column);
    //    int iY = Mathf.RoundToInt(row / 2);

    //    return new CubeIndex(iX, iY, -iX -iY);
    //    //return new CubeIndex(iX, iZ);
    //}
}
