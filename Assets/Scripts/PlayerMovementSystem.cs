using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class PlayerMovementSystem : ComponentSystem
{
    //[Inject] HexTileHightlightComponent hexTileHightlightComponent;

    protected override void OnUpdate()
    {
        var deltaTime = Time.deltaTime;
        Entities.ForEach((Entity entity, Rigidbody rigidbody, InputComponent inputComponent) =>
        {
            var moveVector = new Vector3(inputComponent.Horizontal, 0, inputComponent.Vertical);
            var movePosition = rigidbody.position + moveVector.normalized * 3 * deltaTime;
            rigidbody.MovePosition(movePosition);
        });

        //Entities.ForEach((ref Rigidbody rotationSpeed, ref InputComponent rotation) =>
        //{
        //    var deltaTime = Time.deltaTime;
        //    rotation.Value = math.mul(math.normalize(rotation.Value),
        //        quaternion.AxisAngle(math.up(), rotationSpeed.RadiansPerSecond * deltaTime));
        //});
    }
}
