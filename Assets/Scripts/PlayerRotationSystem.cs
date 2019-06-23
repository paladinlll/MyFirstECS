using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

public class PlayerRotationSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var mousePosition = Input.mousePosition;
        var cameraRay = Camera.main.ScreenPointToRay(mousePosition);
        var layerMask = LayerMask.GetMask("Floor");
        var deltaTime = Time.deltaTime;
        if (Physics.Raycast(cameraRay, out var hit, 100, layerMask))
        {
            Entities.ForEach((Entity entity, Transform transform, ref Rotation rotation) =>
            {
                var forward = hit.point - transform.position;
                var lookAt = Quaternion.LookRotation(forward);
                rotation.Value = new Quaternion(0, lookAt.y, 0, lookAt.w).normalized;
            });
        }
    }
}
