using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Jobs;

public class PlayerRotationSystem : JobComponentSystem
{
    private EntityQuery m_BoidGroup;
    private EntityQuery m_TargetGroup;
    protected override void OnCreate()
    {
        m_TargetGroup = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] { ComponentType.ReadOnly<HexTileHightlightComponent>(), ComponentType.ReadOnly<LocalToWorld>() },
        });

        m_BoidGroup = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] { ComponentType.ReadOnly<InputComponent>(),
                ComponentType.ReadOnly<LocalToWorld>(),
                ComponentType.ReadOnly<Rotation>()},
        });
        base.OnCreate();
    }

    [BurstCompile]
    struct CopyPositions : IJobForEachWithEntity<LocalToWorld>
    {
        public NativeArray<float3> positions;

        public void Execute(Entity entity, int index, [ReadOnly]ref LocalToWorld localToWorld)
        {
            positions[index] = localToWorld.Position;
        }
    }

    [BurstCompile]
    struct Steer : IJobForEachWithEntity<LocalToWorld, Rotation>
    {
        [ReadOnly] public NativeArray<float3> targetPositions;

        public void Execute(Entity entity, int index,[ReadOnly] ref LocalToWorld localToWorld, ref Rotation rotation)
        {
            var forward = targetPositions[0] - localToWorld.Position;
            var lookAt = Quaternion.LookRotation(forward);
            rotation.Value = new Quaternion(0, lookAt.y, 0, lookAt.w).normalized;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        //var targetCount = m_TargetGroup.CalculateLength();
        //if (targetCount != 1) return inputDeps;
        //var copyTargetPositions = new NativeArray<float3>(targetCount, Allocator.TempJob,
        //            NativeArrayOptions.UninitializedMemory);
        //var copyTargetPositionsJob = new CopyPositions
        //{
        //    positions = copyTargetPositions
        //};
        //var copyTargetPositionsJobHandle = copyTargetPositionsJob.Schedule(m_TargetGroup, inputDeps);

        //var steerJob = new Steer
        //{
        //    targetPositions = copyTargetPositions,
        //};
        //var steerJobHandle = steerJob.Schedule(m_BoidGroup, copyTargetPositionsJobHandle);
        //steerJobHandle.Complete();
        //copyTargetPositions.Dispose();
        return inputDeps;
        //var mousePosition = Input.mousePosition;
        //var cameraRay = Camera.main.ScreenPointToRay(mousePosition);
        //var layerMask = LayerMask.GetMask("Floor");
        //var deltaTime = Time.deltaTime;
        //if (Physics.Raycast(cameraRay, out var hit, 100, layerMask))
        //{
        //    Entities.ForEach((Entity entity, Transform transform, ref Rotation rotation) =>
        //    {
        //        var forward = hit.point - transform.position;
        //        var lookAt = Quaternion.LookRotation(forward);
        //        rotation.Value = new Quaternion(0, lookAt.y, 0, lookAt.w).normalized;
        //    });
        //}
    }
}
