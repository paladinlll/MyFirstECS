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
        Entities.With(query).ForEach((Entity entity) =>
       {
           PostUpdateCommands.SetComponent(entity, new InputComponent
           {
               Move = new float2(horizontal, vertical)
           });
       });
    }
}
