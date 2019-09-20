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
        //var mainCamera = Camera.main;
        var deltaTime = Time.deltaTime;
        //var camForward = Vector3.Scale(mainCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
        Entities.With(query).ForEach(
            (Entity entity, Rigidbody rigidBody, ref InputComponent inputComponent) =>
        {
            //var move = inputComponent.Move;
            //if (math.abs(move.x) > 0.0001f || math.abs(move.y) > 0.0001f)
            //{
            //    var movement = new Vector3(move.x, 0, move.y);
            //    movement = movement.normalized * 5 * deltaTime;
            //    var position = rigidBody.transform.position;

            //    var newPos = new Vector3(position.x, position.y, position.z) + movement;
            //    //var movePosition = rigidBody.position + movement;
            //    rigidBody.MovePosition(newPos);


            //    Vector3 v = (movement * 5) / Time.deltaTime;

            //    // we preserve the existing y part of the current velocity.
            //    v.y = rigidBody.velocity.y;
            //    rigidBody.velocity = v;
            //} else {
            //    rigidBody.velocity = Vector3.zero;
            //}
        });
    }
}

