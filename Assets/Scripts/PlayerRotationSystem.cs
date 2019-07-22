using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

public class PlayerRotationSystem : ComponentSystem
{
    private EntityQuery playerQuery;
    private EntityQuery selectingHexQuery;

    protected override void OnCreate()
    {
        playerQuery = GetEntityQuery(
            ComponentType.ReadOnly<Transform>(),
            ComponentType.ReadOnly<InputComponent>(),
            ComponentType.ReadOnly<Rotation>(),
            ComponentType.ReadOnly<Rigidbody>()
            //,ComponentType.Exclude<DeadData>()
            );
        selectingHexQuery = GetEntityQuery(
            ComponentType.ReadOnly<HexTileHightlightComponent>(),
            ComponentType.ReadOnly<Translation>());
    }

    protected override void OnUpdate()
    {
        var hightlightTranslations = selectingHexQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        if (hightlightTranslations.Length == 0)
        {
            hightlightTranslations.Dispose();
            return;
        }
        var cursorPos = hightlightTranslations[0].Value;
        Entities.With(playerQuery).ForEach((Transform transform, ref Rotation rotation) =>
        {
            var position = transform.position;
            var playerToMouse = cursorPos - new float3(position.x, position.y, position.z);
            playerToMouse.y = 0f;
            var lookAt = Quaternion.LookRotation(playerToMouse);
            rotation.Value = new Quaternion(0, lookAt.y, 0, lookAt.w).normalized;
        });
        hightlightTranslations.Dispose();
    }
}
