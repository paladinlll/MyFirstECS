using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

[DisableAutoCreation]
public class PlayerShootingSystem : JobComponentSystem
{
    EntityCommandBufferSystem m_Barrier;

    protected override void OnCreate()
    {
        // Cache the BeginInitializationEntityCommandBufferSystem in a field, so we don't have to create it every frame
        m_Barrier = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    [ExcludeComponent(typeof(Firing))]
    private struct PlayerShootingJob : IJobForEachWithEntity<Weapon>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public bool IsFiring;
        public float CurrentTime;
        public void Execute(Entity entity, int index, [ReadOnly] ref Weapon weapon)
        {
            if (!IsFiring) return;
            CommandBuffer.AddComponent(index, entity, new Firing { FireAt = CurrentTime });
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var commandBuffer = m_Barrier.CreateCommandBuffer().ToConcurrent();
        var job = new PlayerShootingJob
        {
            CommandBuffer = commandBuffer,
            IsFiring = Input.GetButton("Fire1"),
            CurrentTime = Time.time
        }.Schedule(this, inputDeps);
        m_Barrier.AddJobHandleForProducer(job);
        return job;
    }
}
