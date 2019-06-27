using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
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

        Triangulate(Vector3.zero);
        hexMesh.vertices = vertices.ToArray();
        //hexMesh.uv = uvs.ToArray();
        hexMesh.triangles = triangles.ToArray();
        hexMesh.RecalculateNormals();
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
        Vector3 pos = Vector3.zero;
        pos.x = 1 * 3.0f / 2.0f * cell.index.x;
        pos.z = 1 * Mathf.Sqrt(3.0f) * (cell.index.y + cell.index.x / 2.0f);
        Triangulate(pos);
    }

    void Triangulate(Vector3 origin)
    {
        int last = vertices.Count;

        for (int i = 0; i < 6; i++)
            vertices.Add(Corner(origin, 0.9f, i, HexOrientation.Flat));

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

    private struct RenderData
    {
        public Entity entity;
        public float3 position;
        public Matrix4x4 matrix;
    }

    [BurstCompile]
    private struct CopyTileJob : IJobForEachWithEntity<Translation, HexTileComponent>
    {
        public NativeQueue<RenderData>.Concurrent nativeQueue;

        public void Execute(Entity entity, int index, ref Translation translation, ref HexTileComponent hexTileComponent)
        {
            RenderData renderData = new RenderData
            {
                entity = entity,
                position = translation.Value,
                matrix = Matrix4x4.TRS(
                        translation.Value,
                        Quaternion.identity,
                        Vector3.one)
            };
            nativeQueue.Enqueue(renderData);
        }
    }

    [BurstCompile]
    private struct NativeQueueToArrayJob : IJob
    {
        public NativeQueue<RenderData> nativeQueue;
        public NativeArray<RenderData> nativeArray;

        public void Execute()
        {
            int index = 0;
            RenderData renderData;
            while(nativeQueue.TryDequeue(out renderData))
            {
                nativeArray[index] = renderData;
                index++;
            }
        }
    }

    [BurstCompile]
    private struct FillArrayForParalleJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<RenderData> nativeArray;
        [NativeDisableContainerSafetyRestriction] public NativeArray<Matrix4x4> matrixArray;

        public void Execute(int index)
        {
            matrixArray[index] = nativeArray[index].matrix;
        }
    }


    protected override void OnUpdate()
    {
        NativeQueue<RenderData> nativeQueue = new NativeQueue<RenderData>(Allocator.TempJob);

        CopyTileJob copyTileJob = new CopyTileJob
        {
            nativeQueue = nativeQueue.ToConcurrent()
        };
        JobHandle jobHandle = copyTileJob.Schedule(this);
        jobHandle.Complete();

        NativeArray<RenderData> nativeArray = new NativeArray<RenderData>(nativeQueue.Count, Allocator.TempJob);

        NativeQueueToArrayJob nativeQueueToArrayJob = new NativeQueueToArrayJob
        {
            nativeQueue = nativeQueue,
            nativeArray = nativeArray,
        };
        jobHandle = nativeQueueToArrayJob.Schedule();
        jobHandle.Complete();
        NativeArray<Matrix4x4> matrixArray = new NativeArray<Matrix4x4>(nativeArray.Length, Allocator.TempJob);

        FillArrayForParalleJob fillArrayForParalleJob = new FillArrayForParalleJob
        {
            matrixArray = matrixArray,
            nativeArray = nativeArray,
        };
        jobHandle = fillArrayForParalleJob.Schedule(nativeArray.Length, 10);
        jobHandle.Complete();

        nativeQueue.Dispose();

        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        int colorPropertyId = Shader.PropertyToID("_Color");

        int sliceCount = 1023;
        Matrix4x4[] matrixInstanceArray = new Matrix4x4[sliceCount];
        for(int i=0;i< nativeArray.Length;i+= sliceCount)
        {
            int sliceSize = math.min(nativeArray.Length - i, sliceCount);
            NativeArray<Matrix4x4>.Copy(matrixArray, i, matrixInstanceArray, 0, sliceSize);

            materialPropertyBlock.SetColor(colorPropertyId, Color.white);
            Graphics.DrawMeshInstanced(
                hexMesh,
                0,
                Boostrap.YellowMaterial,
                matrixInstanceArray,
                sliceSize,
                materialPropertyBlock);
        }

        matrixArray.Dispose();
        nativeArray.Dispose();
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
