using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

public class HexMeshBuilderSystem : ComponentSystem
{
    private EntityQuery m_MainGroup;
    private EntityQuery m_TargetGroup;

    Mesh hexMesh;
    List<Vector3> vertices;
    List<Vector2> uvs;
    List<int> triangles;
    protected override void OnCreate()
    {
        //m_MainGroup = GetEntityQuery(ComponentType.ReadWrite<HexTileComponent>());
        //m_TargetGroup = GetEntityQuery(ComponentType.ReadWrite<HexTileComponent>());
        //m_TargetGroup.SetFilterChanged(ComponentType.ReadWrite<HexTileComponent>());

        hexMesh = new Mesh();
        vertices = new List<Vector3>();
        uvs = new List<Vector2>();
        triangles = new List<int>();
    }

    void Triangulate(HexTileComponent[] cells)
    {
        hexMesh.Clear();
        vertices.Clear();
        uvs.Clear();
        triangles.Clear();
        for (int i = 0; i < cells.Length; i++)
        {
            Triangulate(ref cells[i]);
        }
        hexMesh.vertices = vertices.ToArray();
        hexMesh.uv = uvs.ToArray();
        hexMesh.triangles = triangles.ToArray();
        hexMesh.RecalculateNormals();

        Boostrap.HexTerrain.mesh = hexMesh;
    }

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

    void Triangulate(ref HexTileComponent cell)
    {
        int last = vertices.Count;
        Vector3 pos = Vector3.zero;
        pos.x = 1 * 3.0f / 2.0f * cell.index.x;
        pos.z = 1 * Mathf.Sqrt(3.0f) * (cell.index.y + cell.index.x / 2.0f);
        for (int i = 0; i < 6; i++)
            vertices.Add(Corner(pos, 0.9f, i, HexOrientation.Flat));

        triangles.Add(last + 0);
        triangles.Add(last + 2);
        triangles.Add(last + 1);

        triangles.Add(last + 0);
        triangles.Add(last + 5);
        triangles.Add(last + 2);

        triangles.Add(last + 2);
        triangles.Add(last + 5);
        triangles.Add(last + 3);

        triangles.Add(last + 3);
        triangles.Add(last + 5);
        triangles.Add(last + 4);

        //UVs are wrong, I need to find an equation for calucalting them
        uvs.Add(new Vector2(0.5f, 1f));
        uvs.Add(new Vector2(1, 0.75f));
        uvs.Add(new Vector2(1, 0.25f));
        uvs.Add(new Vector2(0.5f, 0));
        uvs.Add(new Vector2(0, 0.25f));
        uvs.Add(new Vector2(0, 0.75f));
    }

    protected override void OnUpdate()
    {
        hexMesh.Clear();
        vertices.Clear();
        uvs.Clear();
        triangles.Clear();
        Entities.ForEach((Entity entity, ref HexTileComponent HexTileComponent) =>
        {
            Triangulate(ref HexTileComponent);
        });
        hexMesh.vertices = vertices.ToArray();
        hexMesh.uv = uvs.ToArray();
        hexMesh.triangles = triangles.ToArray();
        hexMesh.RecalculateNormals();

        Boostrap.HexTerrain.mesh = hexMesh;

        Entities.ForEach((Entity entity, RenderMesh renderMesh, ref HexTerrainComponent HexTerrainComponent) =>
        {
            PostUpdateCommands.SetSharedComponent(entity, Boostrap.HexTerrain);
        });
        return;
        var targetCount = m_MainGroup.CalculateLength();
        if(targetCount == 0)
        {
            return;
        }
        var groupEntities = m_MainGroup.ToEntityArray(Allocator.Persistent);
        UnityEngine.Debug.Log(groupEntities.Length);
        hexMesh.Clear();
        vertices.Clear();
        uvs.Clear();
        triangles.Clear();
        foreach (var entity in groupEntities)
        {
            var creator = EntityManager.GetComponentData<HexTileComponent>(entity);
            Triangulate(ref creator);

        }
        hexMesh.vertices = vertices.ToArray();
        hexMesh.uv = uvs.ToArray();
        hexMesh.triangles = triangles.ToArray();
        hexMesh.RecalculateNormals();

        Boostrap.HexTerrain.mesh = hexMesh;


        groupEntities.Dispose();
    }

    //struct CopyHexTiles : IJobForEachWithEntity<HexTileComponent>
    //{
    //    public NativeArray<CubeIndex> CubeIndexs;
    //    public void Execute(Entity entity, int index, [ReadOnly] ref HexTileComponent tile)
    //    {
    //        CubeIndexs[index] = tile.index;
    //    }
    //}

    //struct BuildMesh : IJobForEachWithEntity<HexTerrainComponent, RenderMesh>
    //{
    //    [ReadOnly] public NativeArray<CubeIndex> CubeIndexs;

    //    public void Execute(Entity entity, int index, [ReadOnly] ref HexTerrainComponent HexTerrain, ref RenderMesh RenderMesh)
    //    {
    //        throw new System.NotImplementedException();
    //    }
    //}

    //protected override JobHandle OnUpdate(JobHandle inputDeps)
    //{
    //    var targetCount = m_TargetGroup.CalculateLength();
    //    var copyHexTiles = new NativeArray<CubeIndex>(targetCount, Allocator.TempJob,
    //                 NativeArrayOptions.UninitializedMemory);

    //    var copyHexTilesJob = new CopyHexTiles
    //    {
    //        CubeIndexs = copyHexTiles
    //    };
    //    var copyHexTilesJobHandle = copyHexTilesJob.Schedule(m_TargetGroup, inputDeps);

    //    var buildMeshJob = new BuildMesh
    //    {
    //        CubeIndexs = copyHexTiles
    //    };
    //    var buildMeshJobHandle = buildMeshJob.Schedule(this, copyHexTilesJobHandle);

    //    inputDeps = buildMeshJobHandle;
    //    return inputDeps;
    //}
}
