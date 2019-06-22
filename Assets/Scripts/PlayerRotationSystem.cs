using UnityEngine;
using Unity.Entities;

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
            Entities.ForEach((Entity entity, Transform transform, RotationCompnent rotationCompnent) =>
            {
                var forward = hit.point - transform.position;
                var rotation = Quaternion.LookRotation(forward);
                rotationCompnent.Value = new Quaternion(0, rotation.y, 0, rotation.w).normalized;
            });
        }
    }
}
