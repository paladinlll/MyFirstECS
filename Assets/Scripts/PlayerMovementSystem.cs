using Unity.Entities;
using Unity.Mathematics;
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
            var move = inputComponent.Move;
            if (math.abs(move.x) > 0.0001f || math.abs(move.y) > 0.0001f)
            {
                var forward = rigidBody.position - mainCamera.transform.position;
                forward.y = 0;
                var quaternion = Quaternion.LookRotation(forward, Vector3.up);
                var moveVector = quaternion * new Vector3(move.x, 0, move.y);
                moveVector.y = 0;
                var movePosition = rigidBody.position + moveVector.normalized * 5 * deltaTime;
                rigidBody.MovePosition(movePosition);
            }
        });
    }
}
