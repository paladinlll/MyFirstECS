using Unity.Entities;
//using Unity.Physics.Systems;
using Unity.Rendering;
using UnityEngine;
using Unity.Transforms;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Jobs;

namespace Unity.Physics.Extensions
{
    public class HexCursorSystem : ComponentSystem
    {
        private struct TileData
        {
            public Entity entity;
            public CubeIndex index;
        }

        private struct CopyTileJob : IJobForEachWithEntity<HexTileComponent>
        {
            public NativeQueue<TileData>.Concurrent nativeQueue;

            public void Execute(Entity entity, int index, [ReadOnly] ref HexTileComponent hexTileComponent)
            {
                TileData tileData = new TileData
                {
                    entity = entity,
                    index = hexTileComponent.index
                };
                nativeQueue.Enqueue(tileData);
            }
        }
        [BurstCompile]
        private struct NativeQueueToArrayJob : IJob
        {
            public NativeQueue<TileData> nativeQueue;
            public NativeArray<TileData> nativeArray;

            public void Execute()
            {
                int index = 0;
                TileData renderData;
                while (nativeQueue.TryDequeue(out renderData))
                {
                    nativeArray[index] = renderData;
                    index++;
                }
            }
        }

        protected override void OnUpdate()
        {
            NativeQueue<TileData> nativeQueue = new NativeQueue<TileData>(Allocator.TempJob);

            CopyTileJob copyTileJob = new CopyTileJob
            {
                nativeQueue = nativeQueue.ToConcurrent()
            };
            JobHandle jobHandle = copyTileJob.Schedule(this);
            jobHandle.Complete();

            NativeArray<TileData> nativeArray = new NativeArray<TileData>(nativeQueue.Count, Allocator.TempJob);

            NativeQueueToArrayJob nativeQueueToArrayJob = new NativeQueueToArrayJob
            {
                nativeQueue = nativeQueue,
                nativeArray = nativeArray,
            };
            jobHandle = nativeQueueToArrayJob.Schedule();
            jobHandle.Complete();

            nativeQueue.Dispose();

            var camera = Camera.main;
            if (camera != null && Input.GetMouseButtonDown(0))
            {
                var mousePosition = Input.mousePosition;
                var cameraRay = camera.ScreenPointToRay(mousePosition);

                Entities.WithAll<HexTileHightlightComponent>().ForEach((Entity entity) =>
                {
                    PostUpdateCommands.RemoveComponent<HexTileHightlightComponent>(entity);
                });
                var deltaTime = Time.deltaTime;
                if (UnityEngine.Physics.Raycast(cameraRay, out var closestHit, float.PositiveInfinity))
                {
                    Vector3 mapPos = new Vector3(closestHit.point.x, 0, closestHit.point.z);
                    CubeIndex index = HexUtils.FromPosition(mapPos, Bootstrap.Defines.TileRadius);
                    Debug.DrawLine(Vector3.zero, closestHit.point, Color.red, 2.5f);
                    for (int i = 0; i < nativeArray.Length; i++)
                    {
                        if (nativeArray[i].index.Equals(index))
                        {
                            Debug.Log($"Hit {index}, {closestHit.point}");
                            PostUpdateCommands.AddComponent(nativeArray[i].entity, new HexTileHightlightComponent());
                            break;
                        }
                    }


                }

                //if (CollisionWorld.CastRay(rayInput, out var closestHit))
                {
                    //RigidBody hitBody = CollisionWorld.Bodies[closestHit.RigidBodyIndex];
                    //Debug.Log($"Hit {closestHit.Position}");
                    //HexTileComponent hitTile = EntityManager.GetComponentData<HexTileComponent>(hitBody.Entity);
                    //PostUpdateCommands.AddComponent(hitBody.Entity, new HexTileHightlightComponent());

                    //Entities.ForEach((Entity entity, Transform transform, ref Rotation rotation) =>
                    //{
                    //    var forward = closestHit.Position - (float3)transform.position;
                    //    var lookAt = Quaternion.LookRotation(forward);
                    //    rotation.Value = new Quaternion(0, lookAt.y, 0, lookAt.w).normalized;
                    //});
                }
            }

            nativeArray.Dispose();
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
        var camera = Camera.main;
        //MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        //int colorPropertyId = Shader.PropertyToID("_Color");
        //materialPropertyBlock.SetColor(colorPropertyId, Color.red);
        Entities.WithAll<HexTileHightlightComponent>().ForEach((ref Translation translation) =>
       {
           Vector3 pos = translation.Value;
           pos.y += 1;
           Graphics.DrawMesh(
                Bootstrap.Defines.highlightMesh,
                Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one),
                Bootstrap.Defines.YellowMaterial,
                0,
                camera);
           //Graphics.DrawMesh(
           //    Bootstrap.Defines.highlightMesh,
           //    pos,
           //    Quaternion.identity,
           //    Bootstrap.Defines.YellowMaterial,
           //    0,
           //    camera);
       });
    }
}