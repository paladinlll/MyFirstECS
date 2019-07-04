using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class FiringSystem : JobComponentSystem
{
    private EntityQuery _componentGroup;

    protected override void OnCreateManager()
    {
        _componentGroup = GetEntityQuery(ComponentType.ReadWrite<Firing>(),
                                           ComponentType.ReadWrite<LocalToWorld>(),
                                           ComponentType.ReadWrite<Rotation>());
        _componentGroup.SetFilterChanged(ComponentType.ReadWrite<Firing>());
        base.OnCreateManager();
    }
    EntityCommandBufferSystem m_Barrier;

    protected override void OnCreate()
    {
        // Cache the BeginInitializationEntityCommandBufferSystem in a field, so we don't have to create it every frame
        m_Barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    //public static JobHandle ScheduleBatchRayCast(EntityCommandBuffer.Concurrent commandBuffer,
    //        NativeArray<LocalToWorld> localToWorld, NativeArray<Rotation> rotation)
    //{
    //    JobHandle rcj = new FiringJob
    //    {
    //        CommandBuffer = commandBuffer,
    //        localToWorld = localToWorld,
    //        rotation = rotation

    //    }.Schedule(localToWorld.Length, 5);
    //    return rcj;
    //}

    //protected override void OnUpdate()
    //{
    //var commandBuffer = m_Barrier.CreateCommandBuffer().ToConcurrent();
    //var localToWorld = _componentGroup.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
    //var rotation = _componentGroup.ToComponentDataArray<Rotation>(Allocator.TempJob);

    //JobHandle rayJobHandle = ScheduleBatchRayCast(commandBuffer, localToWorld, rotation);
    //rayJobHandle.Complete();
    //}

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var commandBuffer = m_Barrier.CreateCommandBuffer().ToConcurrent();
        var job = new FiringJob
        {
            CommandBuffer = commandBuffer,
        }.Schedule(this, inputDeps);
        //Schedule(_componentGroup, inputDeps); -> Schedule(this, inputDeps);
        m_Barrier.AddJobHandleForProducer(job);
        return job;
    }

    private struct FiringJob : IJobForEachWithEntity<Firing, LocalToWorld, Rotation>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        //[ReadOnly] ref Firing firing -> [ChangedFilter] ref Firing firing
        public void Execute(Entity entity, int index, [ChangedFilter] ref Firing firing, [ReadOnly] ref LocalToWorld localToWorld, [ReadOnly] ref Rotation rotation)
        {
            var bulletEntity = CommandBuffer.CreateEntity(index);
            CommandBuffer.AddSharedComponent(index, bulletEntity, Bootstrap.Defines.BullerRenderer);
            CommandBuffer.AddComponent(index, bulletEntity, new LocalToWorld
            {
                Value = float4x4.TRS(
                            localToWorld.Position,
                            rotation.Value,
                            new float3(1.0f, 1.0f, 1.0f))
            });
            CommandBuffer.AddComponent(index, bulletEntity, new MoveForward());
        }
    }

}
