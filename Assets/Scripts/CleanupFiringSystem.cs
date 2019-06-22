using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Jobs;

public class CleanupFiringSystem : JobComponentSystem
{
    EntityCommandBufferSystem m_Barrier;

    protected override void OnCreate()
    {
        // Cache the BeginInitializationEntityCommandBufferSystem in a field, so we don't have to create it every frame
        m_Barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    private struct CleanupFiringJob : IJobForEachWithEntity<Firing>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public float CurrentTime;
        public void Execute(Entity entity, int index, ref Firing weapon)
        {
            if (CurrentTime - weapon.FireAt < 0.5f) return;
            CommandBuffer.RemoveComponent<Firing>(index, entity);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var commandBuffer = m_Barrier.CreateCommandBuffer().ToConcurrent();
        var job = new CleanupFiringJob
        {
            CommandBuffer = commandBuffer,
            CurrentTime = Time.time
        }.Schedule(this, inputDeps);
        m_Barrier.AddJobHandleForProducer(job);
        return job;
    }
}
