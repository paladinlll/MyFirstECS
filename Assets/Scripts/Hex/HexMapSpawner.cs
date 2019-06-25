using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Extensions;
using Collider = Unity.Physics.Collider;
using Material = UnityEngine.Material;
using Mesh = UnityEngine.Mesh;

public class HexMapSpawner : MonoBehaviour
{
    public enum HexOrientation
    {
        Pointy,
        Flat
    }
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

    // Start is called before the first frame update
    void Start()
    {
        var entityManager = World.Active.EntityManager;

        HexOrientation orientation = HexOrientation.Flat;
        var hexMesh = new RenderMesh();
        hexMesh.material = new Material(Shader.Find("Standard"));
        //hexMesh.material = new Material(Boostrap.DefaultMaterial);
        GetHexMesh(0.9f, orientation, ref hexMesh.mesh);
        Vector3 pos = new Vector3(0, -1, 0);

        int mapSize = 5;
        float hexRadius = 1;
        for (int q = -mapSize; q <= mapSize; q++)
        {
            int r1 = Mathf.Max(-mapSize, -q - mapSize);
            int r2 = Mathf.Min(mapSize, -q + mapSize);
            for (int r = r1; r <= r2; r++)
            {
                var entity = entityManager.CreateEntity();
                // Place the instantiated entity in a grid with some noise
                pos.x = hexRadius * 3.0f / 2.0f * q;
                pos.z = hexRadius * Mathf.Sqrt(3.0f) * (r + q / 2.0f);
                entityManager.AddComponentData(entity, new LocalToWorld { });
                entityManager.AddComponentData(entity, new Translation { Value = pos });
                entityManager.AddComponentData(entity, new Rotation { Value = Quaternion.identity });
                //entityManager.AddSharedComponentData(entity, hexMesh);

                var index = new CubeIndex(q, r, -q - r);
                entityManager.AddComponentData(entity, new HexTileComponent
                {
                    index = index
                });
                //float3[] points = new float3[hexMesh.mesh.vertices.Length];
                //for (int i = 0; i < 6; i++)
                //    points[i] = Corner(Vector3.zero, 1, i, orientation);
                //BlobAssetReference<Unity.Physics.Collider> collider = Unity.Physics.MeshCollider.Create(points, hexMesh.mesh.GetIndices(0), CollisionFilter.Default);
                //var colliderComponent = new PhysicsCollider { Value = collider };
                //entityManager.AddComponentData(entity, colliderComponent);
            }
        }

        //var highlightTile = new RenderMesh();
        //highlightTile.material = new Material(Boostrap.YellowMaterial);
        var highlightTile = Boostrap.HexTerrain;
        highlightTile.material = new Material(Boostrap.YellowMaterial);
        //GetHexMesh(1, orientation, ref highlightTile.mesh);
        {
            var entity = entityManager.CreateEntity();
            // Place the instantiated entity in a grid with some noise
            pos.x = 0;
            pos.z = 0;
            entityManager.AddComponentData(entity, new LocalToWorld { });
            entityManager.AddComponentData(entity, new Translation { Value = pos });
            entityManager.AddComponentData(entity, new Rotation { Value = Quaternion.identity });
            entityManager.AddSharedComponentData(entity, highlightTile);
            entityManager.AddComponentData(entity, new HexTerrainComponent());
            //var index = new CubeIndex(0, 0, 0);
            //entityManager.AddComponentData(entity, new HexTileHightlightComponent
            //{
            //    index = index
            //});
        }


    }

}
