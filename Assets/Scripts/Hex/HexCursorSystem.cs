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
            Entities.ForEach((Entity entity, ref Translation translation, ref HexTileComponent hexTileComponent) =>
            {

            });

            var camera = Camera.main;
            if (camera != null)
            {
                var mousePosition = Input.mousePosition;
                var cameraRay = camera.ScreenPointToRay(mousePosition);
                var rayInput = new RaycastInput
                {
                    Start = cameraRay.origin,
                    End = cameraRay.origin + cameraRay.direction * k_MaxDistance,
                    Filter = CollisionFilter.Default,
                };
                var CollisionWorld = m_BuildPhysicsWorldSystem.PhysicsWorld.CollisionWorld;

                Entities.WithAll<HexTileHightlightComponent>().ForEach((Entity entity) =>
                {
                    PostUpdateCommands.RemoveComponent<HexTileHightlightComponent>(entity);
                });
                if (CollisionWorld.CastRay(rayInput, out var closestHit))
                {
                    RigidBody hitBody = CollisionWorld.Bodies[closestHit.RigidBodyIndex];
                    HexTileComponent hitTile = EntityManager.GetComponentData<HexTileComponent>(hitBody.Entity);
                    PostUpdateCommands.AddComponent(hitBody.Entity, new HexTileHightlightComponent());

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

public class RenderCursorSystem : ComponentSystem
{
    //Mesh highlightMesh;
    protected override void OnCreate()
    {
        //highlightMesh = new Mesh();
        //HexUtils.GetHexMesh(1, Bootstrap.Defines.orientation, ref highlightMesh);
        base.OnCreate();
    }
    protected override void OnUpdate()
    {
       // var camera = Camera.main;
       // MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
       // int colorPropertyId = Shader.PropertyToID("_Color");
       // materialPropertyBlock.SetColor(colorPropertyId, Color.red);
       // Entities.WithAll<HexTileHightlightComponent>().ForEach((ref Translation translation) =>
       //{
       //    Vector3 pos = translation.Value;
       //    pos.y += 0.001f;
       //    Graphics.DrawMesh(
       //        highlightMesh,
       //        pos,
       //        Quaternion.identity,
       //        Bootstrap.Defines.YellowMaterial,
       //        0,
       //        camera,
       //        0,
       //        materialPropertyBlock);
       //});
    }
}