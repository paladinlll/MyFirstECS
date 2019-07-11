using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

//[DisableAutoCreation]
public class CameraFollowSystem : ComponentSystem
{
    private EntityQuery query;

    private bool firstFrame = true;
    private Vector3 offset;

    protected override void OnCreate()
    {
        query = GetEntityQuery(
            ComponentType.ReadOnly<Transform>(),
            ComponentType.ReadOnly<InputComponent>());
    }

    protected override void OnUpdate()
    {
        var mainCamera = Camera.main;
        if (mainCamera == null)
            return;

        Entities.With(query).ForEach(
            (Entity entity, Transform transform, ref InputComponent data) =>
            {
                var go = transform.gameObject;
                var playerPos = go.transform.position;

                if (firstFrame)
                {
                    offset = mainCamera.transform.position;
                    firstFrame = false;
                }

                var smoothing = 50;
                var dt = Time.deltaTime;
                if (math.abs(data.RotateSpeed) > 0.001f)
                {
                    mainCamera.transform.RotateAround(playerPos, Vector3.up, data.RotateSpeed * 50 * dt);
                    offset = mainCamera.transform.position - playerPos;
                }
                if (math.abs(data.ZoomSpeed) > 0.001f)
                {
                    offset.y = math.clamp(offset.y + data.ZoomSpeed * 30 * dt, 2, 10);
                }

                var targetCamPos = playerPos + offset;

                mainCamera.transform.position =
                    Vector3.Lerp(mainCamera.transform.position, targetCamPos, smoothing * dt);
            });
    }
}