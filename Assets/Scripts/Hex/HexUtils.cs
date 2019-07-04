using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum HexOrientation
{
    Pointy,
    Flat
}

public class HexUtils
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
}
