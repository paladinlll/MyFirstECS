using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class InputSystem : ComponentSystem
{
    private EntityQuery query;

    protected override void OnCreate()
    {
        query = GetEntityQuery(
            ComponentType.ReadOnly<InputComponent>());
    }

    protected override void OnUpdate()
    {
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");
        var zoomSpeed = Input.GetAxis("Mouse ScrollWheel");
        if(math.abs(zoomSpeed) < 0.001f)
        {
            zoomSpeed = 0;
        }
        else
        {
            zoomSpeed = math.sign(zoomSpeed);
        }
        if (Input.GetKey(KeyCode.PageUp))
        {
            zoomSpeed += 1;
        }
        if (Input.GetKey(KeyCode.PageDown))
        {
            zoomSpeed -= 1;
        }
        var rotateSpeed = 0;
        if (Input.GetKey(KeyCode.Q))
        {
            rotateSpeed -= 1;
        }
        if (Input.GetKey(KeyCode.E))
        {
            rotateSpeed += 1;
        }

        Entities.With(query).ForEach((Entity entity) =>
       {
           float3 move = vertical * Vector3.forward + horizontal * Vector3.right;
           PostUpdateCommands.SetComponent(entity, new InputComponent
           {
               Move = new float2(move.x, move.z),
               ZoomSpeed = zoomSpeed,
               RotateSpeed = rotateSpeed
           });
       });
    }
}
