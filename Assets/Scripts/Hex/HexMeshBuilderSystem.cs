using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class HexMeshBuilderSystem : ComponentSystem
{
    private struct RenderData
    {
        public Entity entity;
        public HexTileComponent hexTile;
        public float3 position;
        public Matrix4x4 matrix;
    }

    [BurstCompile]
    private struct CopyTileJob : IJobForEachWithEntity<Translation, HexTileComponent>
    {
        public NativeQueue<RenderData>.ParallelWriter terrain0NativeQueue;
        public NativeQueue<RenderData>.ParallelWriter terrain1NativeQueue;

        public void Execute(Entity entity, int index, ref Translation translation, ref HexTileComponent hexTileComponent)
        {
            RenderData renderData = new RenderData
            {
                entity = entity,
                hexTile = hexTileComponent,
                position = translation.Value,
                matrix = Matrix4x4.TRS(
                        translation.Value,
                        Quaternion.identity,
                        Vector3.one)
            };
            if (hexTileComponent.terrainTypeIndex == 0)
            {
                terrain0NativeQueue.Enqueue(renderData);
            }
            else
            {
                terrain1NativeQueue.Enqueue(renderData);
            }
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
            while (nativeQueue.TryDequeue(out renderData))
            {
                nativeArray[index++] = renderData;
            }
        }
    }

    [BurstCompile]
    private struct FillArrayForParalleJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<RenderData> nativeArray;
        //[ReadOnly] public NativeArray<RenderData> terrain1NativeArray;
        [NativeDisableContainerSafetyRestriction] public NativeArray<Matrix4x4> matrixArray;
        public int startingIndex;

        public void Execute(int index)
        {
            RenderData renderData = nativeArray[index];
            matrixArray[startingIndex + index] = renderData.matrix;
        }
    }

    Matrix4x4[] matrixInstanceArray = new Matrix4x4[1023];
    protected override void OnUpdate()
    {
        NativeQueue<RenderData> terrain0NativeQueue = new NativeQueue<RenderData>(Allocator.TempJob);
        NativeQueue<RenderData> terrain1NativeQueue = new NativeQueue<RenderData>(Allocator.TempJob);

        CopyTileJob copyTileJob = new CopyTileJob
        {
            terrain0NativeQueue = terrain0NativeQueue.AsParallelWriter(),
            terrain1NativeQueue = terrain1NativeQueue.AsParallelWriter()
        };
        JobHandle jobHandle = copyTileJob.Schedule(this);
        jobHandle.Complete();

        NativeArray<RenderData> terrain0NativeArray = new NativeArray<RenderData>(terrain0NativeQueue.Count, Allocator.TempJob);
        NativeArray<RenderData> terrain1NativeArray = new NativeArray<RenderData>(terrain1NativeQueue.Count, Allocator.TempJob);

        NativeArray<JobHandle> jobHandleArray = new NativeArray<JobHandle>(2, Allocator.TempJob);

        NativeQueueToArrayJob terrain0NativeQueueToArrayJob = new NativeQueueToArrayJob
        {
            nativeQueue = terrain0NativeQueue,
            nativeArray = terrain0NativeArray,
        };
        jobHandleArray[0] = terrain0NativeQueueToArrayJob.Schedule();

        NativeQueueToArrayJob terrain1NativeQueueToArrayJob = new NativeQueueToArrayJob
        {
            nativeQueue = terrain1NativeQueue,
            nativeArray = terrain1NativeArray,
        };
        jobHandleArray[1] = terrain1NativeQueueToArrayJob.Schedule();

        JobHandle.CompleteAll(jobHandleArray);

        terrain0NativeQueue.Dispose();
        terrain1NativeQueue.Dispose();

        int visibleTileTotal = terrain0NativeArray.Length + terrain1NativeArray.Length;

        NativeArray<Matrix4x4> matrixArray = new NativeArray<Matrix4x4>(visibleTileTotal, Allocator.TempJob);

        FillArrayForParalleJob fillArrayForParalleJob_0 = new FillArrayForParalleJob
        {
            matrixArray = matrixArray,
            nativeArray = terrain0NativeArray,
            startingIndex = 0
        };
        jobHandleArray[0] = fillArrayForParalleJob_0.Schedule(terrain0NativeArray.Length, 10);

        FillArrayForParalleJob fillArrayForParalleJob_1 = new FillArrayForParalleJob
        {
            matrixArray = matrixArray,
            nativeArray = terrain1NativeArray,
            startingIndex = terrain0NativeArray.Length
        };
        jobHandleArray[1] = fillArrayForParalleJob_1.Schedule(terrain1NativeArray.Length, 10);

        JobHandle.CompleteAll(jobHandleArray);

        int sliceCount = matrixInstanceArray.Length;
        int off = 0;
        //for(int i=0;i< nativeArray.Length;i+= sliceCount)
        while (off < visibleTileTotal)
        {
            float tilePick = off < terrain0NativeArray.Length ? 0 : 0.34f;
            int sliceSize = math.min(visibleTileTotal - off, sliceCount);
            if (off < terrain0NativeArray.Length && off + sliceSize >= terrain0NativeArray.Length)
            {
                sliceSize = terrain0NativeArray.Length - off;
            }
            NativeArray<Matrix4x4>.Copy(matrixArray, off, matrixInstanceArray, 0, sliceSize);

            Graphics.DrawMeshInstanced(
                Bootstrap.Defines.tileCollections.Pick(tilePick),
                0,
                Bootstrap.Defines.TileMaterial,
                matrixInstanceArray,
                sliceSize);

            off += sliceSize;
        }

        matrixArray.Dispose();
        terrain0NativeArray.Dispose();
        terrain1NativeArray.Dispose();
        jobHandleArray.Dispose();
    }
}
