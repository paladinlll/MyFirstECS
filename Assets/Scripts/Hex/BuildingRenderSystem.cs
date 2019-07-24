using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisableAutoCreation]
public class BuildingRenderSystem : ComponentSystem
{
    private struct RenderData
    {
        public Entity entity;
        public ResourceNodeComponent resourceNode;
        public float3 position;
        public Matrix4x4 matrix;
    }

    [BurstCompile]
    private struct CopyTileJob : IJobForEachWithEntity<Translation, ResourceNodeComponent>
    {
        public NativeQueue<RenderData>.Concurrent node0NativeQueue;
        public NativeQueue<RenderData>.Concurrent node1NativeQueue;
        public NativeQueue<RenderData>.Concurrent node2NativeQueue;
        public NativeQueue<RenderData>.Concurrent node3NativeQueue;
        public NativeQueue<RenderData>.Concurrent node4NativeQueue;
        public NativeQueue<RenderData>.Concurrent node5NativeQueue;
        public NativeQueue<RenderData>.Concurrent node6NativeQueue;
        public NativeQueue<RenderData>.Concurrent node7NativeQueue;

        public void Execute(Entity entity, int index, ref Translation translation, ref ResourceNodeComponent resourceNode)
        {
            RenderData renderData = new RenderData
            {
                entity = entity,
                resourceNode = resourceNode,
                position = translation.Value,
                matrix = Matrix4x4.TRS(
                        translation.Value,
                        Quaternion.identity,
                        Vector3.one)
            };
            switch (resourceNode.ResourceId)
            {
                case 1: node1NativeQueue.Enqueue(renderData); break;
                case 2: node2NativeQueue.Enqueue(renderData); break;
                case 3: node3NativeQueue.Enqueue(renderData); break;
                case 4: node4NativeQueue.Enqueue(renderData); break;
                case 5: node5NativeQueue.Enqueue(renderData); break;
                case 6: node6NativeQueue.Enqueue(renderData); break;
                default: node0NativeQueue.Enqueue(renderData); break;
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
        [NativeDisableContainerSafetyRestriction] public NativeArray<Matrix4x4> matrixArray;
        public int startingIndex;

        public void Execute(int index)
        {
            RenderData renderData = nativeArray[index];
            matrixArray[startingIndex + index] = renderData.matrix;
        }
    }

    protected override void OnUpdate()
    {
        NativeQueue<RenderData> node0NativeQueue = new NativeQueue<RenderData>(Allocator.TempJob);
        NativeQueue<RenderData> node1NativeQueue = new NativeQueue<RenderData>(Allocator.TempJob);
        NativeQueue<RenderData> node2NativeQueue = new NativeQueue<RenderData>(Allocator.TempJob);
        NativeQueue<RenderData> node3NativeQueue = new NativeQueue<RenderData>(Allocator.TempJob);
        NativeQueue<RenderData> node4NativeQueue = new NativeQueue<RenderData>(Allocator.TempJob);
        NativeQueue<RenderData> node5NativeQueue = new NativeQueue<RenderData>(Allocator.TempJob);
        NativeQueue<RenderData> node6NativeQueue = new NativeQueue<RenderData>(Allocator.TempJob);

        CopyTileJob copyTileJob = new CopyTileJob
        {
            node0NativeQueue = node0NativeQueue.ToConcurrent(),
            node1NativeQueue = node1NativeQueue.ToConcurrent(),
            node2NativeQueue = node2NativeQueue.ToConcurrent(),
            node3NativeQueue = node3NativeQueue.ToConcurrent(),
            node4NativeQueue = node4NativeQueue.ToConcurrent(),
            node5NativeQueue = node5NativeQueue.ToConcurrent(),
            node6NativeQueue = node6NativeQueue.ToConcurrent()
        };
        JobHandle jobHandle = copyTileJob.Schedule(this);
        jobHandle.Complete();

        NativeArray<RenderData> node0NativeArray = new NativeArray<RenderData>(node0NativeQueue.Count, Allocator.TempJob);
        NativeArray<RenderData> node1NativeArray = new NativeArray<RenderData>(node1NativeQueue.Count, Allocator.TempJob);
        NativeArray<RenderData> node2NativeArray = new NativeArray<RenderData>(node1NativeQueue.Count, Allocator.TempJob);
        NativeArray<RenderData> node3NativeArray = new NativeArray<RenderData>(node1NativeQueue.Count, Allocator.TempJob);
        NativeArray<RenderData> node4NativeArray = new NativeArray<RenderData>(node1NativeQueue.Count, Allocator.TempJob);
        NativeArray<RenderData> node5NativeArray = new NativeArray<RenderData>(node1NativeQueue.Count, Allocator.TempJob);
        NativeArray<RenderData> node6NativeArray = new NativeArray<RenderData>(node1NativeQueue.Count, Allocator.TempJob);

        NativeArray<JobHandle> jobHandleArray = new NativeArray<JobHandle>(7, Allocator.TempJob);

        NativeQueueToArrayJob node0NativeQueueToArrayJob = new NativeQueueToArrayJob
        {
            nativeQueue = node0NativeQueue,
            nativeArray = node0NativeArray,
        };
        jobHandleArray[0] = node0NativeQueueToArrayJob.Schedule();

        NativeQueueToArrayJob node1NativeQueueToArrayJob = new NativeQueueToArrayJob
        {
            nativeQueue = node1NativeQueue,
            nativeArray = node1NativeArray,
        };
        jobHandleArray[1] = node1NativeQueueToArrayJob.Schedule();

        NativeQueueToArrayJob node2NativeQueueToArrayJob = new NativeQueueToArrayJob
        {
            nativeQueue = node2NativeQueue,
            nativeArray = node2NativeArray,
        };
        jobHandleArray[2] = node2NativeQueueToArrayJob.Schedule();

        NativeQueueToArrayJob node3NativeQueueToArrayJob = new NativeQueueToArrayJob
        {
            nativeQueue = node3NativeQueue,
            nativeArray = node3NativeArray,
        };
        jobHandleArray[3] = node3NativeQueueToArrayJob.Schedule();

        NativeQueueToArrayJob node4NativeQueueToArrayJob = new NativeQueueToArrayJob
        {
            nativeQueue = node4NativeQueue,
            nativeArray = node4NativeArray,
        };
        jobHandleArray[4] = node4NativeQueueToArrayJob.Schedule();

        NativeQueueToArrayJob node5NativeQueueToArrayJob = new NativeQueueToArrayJob
        {
            nativeQueue = node5NativeQueue,
            nativeArray = node5NativeArray,
        };
        jobHandleArray[5] = node5NativeQueueToArrayJob.Schedule();

        NativeQueueToArrayJob node6NativeQueueToArrayJob = new NativeQueueToArrayJob
        {
            nativeQueue = node6NativeQueue,
            nativeArray = node6NativeArray,
        };
        jobHandleArray[6] = node6NativeQueueToArrayJob.Schedule();

        JobHandle.CompleteAll(jobHandleArray);

        node0NativeQueue.Dispose();
        node1NativeQueue.Dispose();
        node2NativeQueue.Dispose();
        node3NativeQueue.Dispose();
        node4NativeQueue.Dispose();
        node5NativeQueue.Dispose();
        node6NativeQueue.Dispose();

        int visibleTileTotal = node0NativeArray.Length + node1NativeArray.Length;

        NativeArray<Matrix4x4> matrixArray = new NativeArray<Matrix4x4>(visibleTileTotal, Allocator.TempJob);

        int startingIndex = 0;
        FillArrayForParalleJob fillArrayForParalleJob_0 = new FillArrayForParalleJob
        {
            matrixArray = matrixArray,
            nativeArray = node0NativeArray,
            startingIndex = startingIndex
        };
        jobHandleArray[0] = fillArrayForParalleJob_0.Schedule(node0NativeArray.Length, 10);
        startingIndex += node0NativeArray.Length;

        FillArrayForParalleJob fillArrayForParalleJob_1 = new FillArrayForParalleJob
        {
            matrixArray = matrixArray,
            nativeArray = node1NativeArray,
            startingIndex = startingIndex
        };
        jobHandleArray[1] = fillArrayForParalleJob_1.Schedule(node1NativeArray.Length, 10);
        startingIndex += node1NativeArray.Length;

        FillArrayForParalleJob fillArrayForParalleJob_2 = new FillArrayForParalleJob
        {
            matrixArray = matrixArray,
            nativeArray = node2NativeArray,
            startingIndex = startingIndex
        };
        jobHandleArray[2] = fillArrayForParalleJob_2.Schedule(node1NativeArray.Length, 10);
        startingIndex += node2NativeArray.Length;

        FillArrayForParalleJob fillArrayForParalleJob_3 = new FillArrayForParalleJob
        {
            matrixArray = matrixArray,
            nativeArray = node1NativeArray,
            startingIndex = startingIndex
        };
        jobHandleArray[3] = fillArrayForParalleJob_3.Schedule(node3NativeArray.Length, 10);
        startingIndex += node3NativeArray.Length;

        FillArrayForParalleJob fillArrayForParalleJob_4 = new FillArrayForParalleJob
        {
            matrixArray = matrixArray,
            nativeArray = node4NativeArray,
            startingIndex = startingIndex
        };
        jobHandleArray[4] = fillArrayForParalleJob_4.Schedule(node4NativeArray.Length, 10);
        startingIndex += node4NativeArray.Length;

        FillArrayForParalleJob fillArrayForParalleJob_5 = new FillArrayForParalleJob
        {
            matrixArray = matrixArray,
            nativeArray = node5NativeArray,
            startingIndex = startingIndex
        };
        jobHandleArray[5] = fillArrayForParalleJob_5.Schedule(node5NativeArray.Length, 10);
        startingIndex += node5NativeArray.Length;

        FillArrayForParalleJob fillArrayForParalleJob_6 = new FillArrayForParalleJob
        {
            matrixArray = matrixArray,
            nativeArray = node6NativeArray,
            startingIndex = startingIndex
        };
        jobHandleArray[6] = fillArrayForParalleJob_6.Schedule(node6NativeArray.Length, 10);
        startingIndex += node6NativeArray.Length;

        JobHandle.CompleteAll(jobHandleArray);

        int sliceCount = 1023;
        Matrix4x4[] matrixInstanceArray = new Matrix4x4[1023];
        int off = 0;
        //while (off < visibleTileTotal)
        //{
        //    int tilePick = off < node0NativeArray.Length ? 0 : 0.34f;
        //    int sliceSize = math.min(visibleTileTotal - off, sliceCount);
        //    if (off < node0NativeArray.Length && off + sliceSize >= node0NativeArray.Length)
        //    {
        //        sliceSize = node0NativeArray.Length - off;
        //    }
        //    NativeArray<Matrix4x4>.Copy(matrixArray, off, matrixInstanceArray, 0, sliceSize);

        //    Graphics.DrawMeshInstanced(
        //        Bootstrap.Defines.ResourceNodesPrefab[tilePick].mesh,
        //        0,
        //        Bootstrap.Defines.ResourceNodesPrefab[tilePick].material,
        //        matrixInstanceArray,
        //        sliceSize);

        //    off += sliceSize;
        //}

        matrixArray.Dispose();
        node0NativeArray.Dispose();
        node1NativeArray.Dispose();
        node2NativeArray.Dispose();
        node3NativeArray.Dispose();
        node4NativeArray.Dispose();
        node5NativeArray.Dispose();
        node6NativeArray.Dispose();
        jobHandleArray.Dispose();
    }
}
