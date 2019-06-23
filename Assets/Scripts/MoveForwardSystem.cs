using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

public class MoveForwardSystem : JobComponentSystem
{
    EntityCommandBufferSystem m_Barrier;

    protected override void OnCreate()
    {
        // Cache the BeginInitializationEntityCommandBufferSystem in a field, so we don't have to create it every frame
        m_Barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    private struct MoveForwardJob : IJobForEachWithEntity<LocalToWorld, MoveForward>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public float dt;
        public void Execute(Entity entity, int index,  ref LocalToWorld localToWorld, [ReadOnly] ref MoveForward moveForward)
        {
            localToWorld = new LocalToWorld
            {
                //var lookAt = Quaternion.LookRotation(forward);
                Value = float4x4.TRS(
                        new float3(localToWorld.Position + localToWorld.Forward * dt * 1),
                        Quaternion.LookRotation(localToWorld.Forward),
                        new float3(1.0f, 1.0f, 1.0f))
            };
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var commandBuffer = m_Barrier.CreateCommandBuffer().ToConcurrent();
        var job = new MoveForwardJob
        {
            CommandBuffer = commandBuffer,
            dt = Time.deltaTime
        }.Schedule(this, inputDeps);
        m_Barrier.AddJobHandleForProducer(job);
        return job;
    }
    //protected override void OnUpdate()
    //{
    //    var dist = Time.deltaTime * 1;
    //    Entities.ForEach((Entity entity, ref LocalToWorld localToWorld, ref MoveForward moveForward) =>
    //    {
    //        localToWorld = new LocalToWorld
    //        {
    //            Value = float4x4.TRS(
    //                    new float3(localToWorld.Position + localToWorld.Forward * dist),
    //                    Quaternion.identity,
    //                    new float3(1.0f, 1.0f, 1.0f))
    //        };
    //        //var forward = hit.point - transform.position;
    //        //var lookAt = Quaternion.LookRotation(forward);
    //        //rotation.Value = new Quaternion(0, lookAt.y, 0, lookAt.w).normalized;
    //    });
    //}
}
