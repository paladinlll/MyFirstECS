using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;

public class RotationSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, Rigidbody rigidbody, RotationCompnent rotationCompnent) =>
        {
            var rotation = rotationCompnent.Value;
            rigidbody.MoveRotation(rotation);
        });
    }
}
