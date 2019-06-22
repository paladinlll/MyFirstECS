using Unity.Entities;
using UnityEngine;

public class InputSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");
        Entities.ForEach((Entity entity, InputComponent inputComponent) =>
        {
            inputComponent.Horizontal = horizontal;
            inputComponent.Vertical = vertical;
        });
    }
}
