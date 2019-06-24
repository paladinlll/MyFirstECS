using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Assertions;
using static Unity.Physics.Math;
namespace Unity.Physics.Extensions
{
    [UpdateAfter(typeof(BuildPhysicsWorld)), UpdateBefore(typeof(EndFramePhysicsSystem))]
    public class HexCursorSystem : ComponentSystem
    {
        BuildPhysicsWorld m_BuildPhysicsWorldSystem;
        //EntityQuery m_MouseGroup;
        protected override void OnCreate()
        {
            m_BuildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
            //m_MouseGroup = GetEntityQuery(new EntityQueryDesc
            //{
            //    All = new ComponentType[] { typeof(HexTileComponent) }
            //});

            base.OnCreate();
        }

        const float k_MaxDistance = 100.0f;
        public struct SpringData
        {
            public Entity Entity;
            public int Dragging; // bool isn't blittable
            public RenderMesh OriginRenderMesh;
            //public float3 PointOnBody;
            //public float MouseDepth;
        }

        protected override void OnUpdate()
        {
            //if (m_MouseGroup.CalculateLength() == 0)
            //{
            //    return;
            //}
            if ((Camera.main != null))
            {
                var mousePosition = Input.mousePosition;
                var cameraRay = Camera.main.ScreenPointToRay(mousePosition);
                var rayInput = new RaycastInput
                {
                    Start = cameraRay.origin,
                    End = cameraRay.origin + cameraRay.direction * k_MaxDistance,
                    Filter = CollisionFilter.Default,
                };
                var CollisionWorld = m_BuildPhysicsWorldSystem.PhysicsWorld.CollisionWorld;

                if (CollisionWorld.CastRay(rayInput, out var closestHit))
                {
                    RigidBody hitBody = CollisionWorld.Bodies[closestHit.RigidBodyIndex];
                    HexTileComponent hitTile = EntityManager.GetComponentData<HexTileComponent>(hitBody.Entity);
                    Entities.ForEach((Entity entity, ref HexTileHightlightComponent hightlight) =>
                    {
                        Translation hitTranslation = EntityManager.GetComponentData<Translation>(hitBody.Entity);
                        if (!hightlight.index.Equals(hitTile.index))
                        {
                            hightlight.index = hitTile.index;
                            var newPos = new Translation { Value = hitTranslation.Value };
                            newPos.Value.y -= 0.001f;
                            EntityManager.SetComponentData<Translation>(entity, newPos);
                        }
                    });

                    Entities.ForEach((Entity entity, Transform transform, ref Rotation rotation) =>
                    {
                        var forward = closestHit.Position - (float3)transform.position;
                        var lookAt = Quaternion.LookRotation(forward);
                        rotation.Value = new Quaternion(0, lookAt.y, 0, lookAt.w).normalized;
                    });
                }
            }
        }
    }
}