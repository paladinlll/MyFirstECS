using Unity.Entities;
using UnityEngine;

public class PlayerMovementSystem : ComponentSystem
{
    //[Inject] HexTileHightlightComponent hexTileHightlightComponent;
    private EntityQuery query;

    protected override void OnCreate()
    {
        query = GetEntityQuery(
            ComponentType.ReadOnly<Transform>(),
            ComponentType.ReadOnly<InputComponent>(),
            ComponentType.ReadOnly<Rigidbody>());
    }

    protected override void OnUpdate()
    {
        var mainCamera = Camera.main;
        var deltaTime = Time.deltaTime;
        Entities.With(query).ForEach(
            (Entity entity, Rigidbody rigidBody, ref InputComponent inputComponent) =>
        {
            var forward = rigidBody.position - mainCamera.transform.position;
            forward.y = 0;
            var quaternion = Quaternion.LookRotation(forward, Vector3.up);

            var move = inputComponent.Move;
            var moveVector = quaternion * new Vector3(move.x, 0, move.y);
            var movePosition = rigidBody.position + moveVector.normalized * 3 * deltaTime;
            rigidBody.MovePosition(movePosition);
        });

        //Entities.ForEach((ref Rigidbody rotationSpeed, ref InputComponent rotation) =>
        //{
        //    var deltaTime = Time.deltaTime;
        //    rotation.Value = math.mul(math.normalize(rotation.Value),
        //        quaternion.AxisAngle(math.up(), rotationSpeed.RadiansPerSecond * deltaTime));
        //});
    }
}
