using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;

public class RotationSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, Rigidbody rigidbody, ref  Rotation rotation) =>
        {
            rigidbody.MoveRotation(rotation.Value);
        });
    }
}
