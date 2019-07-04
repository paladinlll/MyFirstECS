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

        int sliceCount = 1023;
        Matrix4x4[] matrixInstanceArray = new Matrix4x4[sliceCount];
        for(int i=0;i< nativeArray.Length;i+= sliceCount)
        {
            int sliceSize = math.min(nativeArray.Length - i, sliceCount);
            NativeArray<Matrix4x4>.Copy(matrixArray, i, matrixInstanceArray, 0, sliceSize);

            Graphics.DrawMeshInstanced(
                Bootstrap.Defines.TileMesh,
                0,
                Bootstrap.Defines.TileMaterial,
                matrixInstanceArray,
                sliceSize);
        }

        matrixArray.Dispose();
        nativeArray.Dispose();
    }
}
